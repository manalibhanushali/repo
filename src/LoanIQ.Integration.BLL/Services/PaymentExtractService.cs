using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using FluentValidation;
using LoanIQ.Integration.DTO;
using Microsoft.Extensions.Logging;

namespace LoanIQ.Integration.BLL.Services;

public sealed record PaymentExtractResult
{
    public bool IsSuccess { get; init; }
    public string? OutputFile { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public int ProcessedCount { get; init; }
    public int SuppressedCount { get; init; }
}

/// Extracts outgoing payments to a single XML file per business date (INTF-15, ART-040).
/// Applies internal-transfer suppression and builds mutually exclusive method blocks.
public sealed class PaymentExtractService(
    IMapper mapper,
    IValidator<PaymentRecord> validator,
    ILogger<PaymentExtractService> logger)
{
    private static readonly XmlSerializer XmlSerializer = new(typeof(PaymentFile));

    public async Task<PaymentExtractResult> ExtractAsync(
        IReadOnlyList<PaymentRecord> payments,
        string businessDate,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);

        // Apply suppression: skip internal transfers (same branch debit and credit).
        var suppressed = 0;
        var unsuppressed = new List<PaymentRecord>();
        foreach (var p in payments)
        {
            if (!string.IsNullOrWhiteSpace(p.CreditBranch) &&
                p.DebitBranch.Equals(p.CreditBranch, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogDebug(
                    "Payment {Reference} suppressed: internal transfer (branch={Branch})",
                    p.PaymentReference, p.DebitBranch);
                suppressed++;
                continue;
            }

            unsuppressed.Add(p);
        }

        var errors = new List<string>();
        var validPayments = new List<PaymentRecord>();

        foreach (var payment in unsuppressed)
        {
            var result = await validator.ValidateAsync(payment, cancellationToken);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                    logger.LogWarning(
                        "Payment {Reference} failed validation: {Error}",
                        payment.PaymentReference, error.ErrorMessage);

                errors.AddRange(result.Errors.Select(e => e.ErrorMessage));
                continue;
            }

            validPayments.Add(payment);
        }

        var xmlRecords = validPayments.Select(p => mapper.Map<PaymentXmlRecord>(p)).ToList();

        var trailingHash = ComputeTrailerHash(xmlRecords.Select(r => r.PaymentReference));

        var file = new PaymentFile
        {
            Header = new PaymentFileHeader
            {
                FileFormatVersion = "0002",
                CreationModule    = "LIQ_SSS",
                CreationDate      = businessDate,
                RecordCount       = xmlRecords.Count.ToString().PadLeft(6, '0'),
            },
            Payments = xmlRecords,
            Trailer  = new PaymentFileTrailer { Hash = trailingHash },
        };

        var filePath = BuildFilePath(outputDirectory, businessDate);
        await WriteXmlAsync(filePath, file, cancellationToken);

        logger.LogInformation(
            "Payment extract file written: {FilePath} ({Count} records, {Suppressed} suppressed)",
            filePath, xmlRecords.Count, suppressed);

        return new PaymentExtractResult
        {
            IsSuccess      = errors.Count == 0,
            OutputFile     = filePath,
            Errors         = errors,
            ProcessedCount = xmlRecords.Count,
            SuppressedCount = suppressed,
        };
    }

    private static string ComputeTrailerHash(IEnumerable<string> paymentReferences)
    {
        var concatenated = string.Concat(paymentReferences);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(concatenated));
        return Convert.ToHexString(bytes); // uppercase hex
    }

    private static string BuildFilePath(string outputDirectory, string businessDate)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"LIQ_TWS_PAYMENT_{businessDate}_{timestamp}.xml";
        return Path.Combine(outputDirectory, fileName);
    }

    private static async Task WriteXmlAsync(string filePath, PaymentFile file, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        var settings = new XmlWriterSettings
        {
            Async           = true,
            Indent          = true,
            Encoding        = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Replace,
        };

        await using var xmlWriter = XmlWriter.Create(stream, settings);
        XmlSerializer.Serialize(xmlWriter, file);
        await xmlWriter.FlushAsync();

        // Ensure final newline before stream closes (cosmetic, not required by schema).
        cancellationToken.ThrowIfCancellationRequested();
    }
}
