using System.Text;
using AutoMapper;
using FluentValidation;
using LoanIQ.Integration.DTO;
using Microsoft.Extensions.Logging;

namespace LoanIQ.Integration.BLL.Services;

public sealed record GlExtractResult
{
    public bool IsSuccess { get; init; }
    public IReadOnlyList<string> OutputFiles { get; init; } = [];
    public IReadOnlyList<string> Errors { get; init; } = [];
    public int ProcessedCount { get; init; }
    public int SkippedCount { get; init; }
}

/// Extracts pending GL entries to pipe-delimited CSV files, one per branch (INTF-06, ART-020).
/// Aborts a branch extraction if debit/credit totals do not balance per voucher.
public sealed class GlExtractService(
    IMapper mapper,
    IValidator<GlEntryRecord> validator,
    ILogger<GlExtractService> logger)
{
    private static readonly string[] CsvHeaders =
    [
        "Company", "JournalName", "JournalBatchNumber", "Voucher",
        "TransDate", "TransEffDate", "Description", "Txt",
        "CurrencyCode", "AmountCurDebit", "AmountCurCredit",
        "AmountMSTDebit", "AmountMSTCredit", "ExchRate",
        "AccountNum", "Dimension1", "Dimension2",
        "CustAccountNum", "CountryRegionId", "State", "CustGroup",
        "ReferenceNum", "ReferenceNum2", "JournalCategory", "LineNum",
    ];

    public async Task<GlExtractResult> ExtractAsync(
        IReadOnlyList<GlEntryRecord> entries,
        string businessDate,
        string outputDirectory,
        IReadOnlyList<string> knownBranches,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDirectory);

        var errors = new List<string>();
        var validEntries = new List<GlEntryRecord>();
        var skipped = 0;

        foreach (var entry in entries)
        {
            var result = await validator.ValidateAsync(entry, cancellationToken);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                    logger.LogWarning("GL entry {LineNum} failed validation: {Error}", entry.LineNum, error.ErrorMessage);

                errors.AddRange(result.Errors.Select(e => e.ErrorMessage));
                skipped++;
                continue;
            }

            validEntries.Add(entry);
        }

        var byBranch = validEntries.GroupBy(e => e.Branch).ToDictionary(g => g.Key, g => g.ToList());

        // Include configured known branches even if there are no entries (zero-row file).
        var allBranches = byBranch.Keys.Union(knownBranches, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var outputFiles = new List<string>();

        foreach (var branch in allBranches.OrderBy(b => b))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var branchEntries = byBranch.TryGetValue(branch, out var found) ? found : [];

            var imbalance = CheckVoucherBalance(branch, branchEntries);
            if (imbalance != null)
            {
                var msg = $"Branch {branch}: {imbalance}";
                logger.LogError("GL extract aborted for branch {Branch}: {Reason}", branch, imbalance);
                errors.Add(msg);
                continue;
            }

            var rows = branchEntries.Select(e => mapper.Map<GlExtractRow>(e)).ToList();
            var filePath = BuildFilePath(outputDirectory, branch, businessDate);

            await WriteCsvAsync(filePath, rows, cancellationToken);

            logger.LogInformation(
                "GL extract file written: {FilePath} ({RowCount} rows)",
                filePath, rows.Count);

            outputFiles.Add(filePath);
        }

        return new GlExtractResult
        {
            IsSuccess = errors.Count == 0,
            OutputFiles = outputFiles,
            Errors = errors,
            ProcessedCount = validEntries.Count,
            SkippedCount = skipped,
        };
    }

    private static string? CheckVoucherBalance(string branch, IReadOnlyList<GlEntryRecord> entries)
    {
        foreach (var voucherGroup in entries.GroupBy(e => e.Voucher))
        {
            var debits = voucherGroup
                .Where(e => e.DebitCreditIndicator == "D")
                .Sum(e => Math.Abs(e.TransAmount));

            var credits = voucherGroup
                .Where(e => e.DebitCreditIndicator == "C")
                .Sum(e => Math.Abs(e.TransAmount));

            if (debits != credits)
                return $"Voucher {voucherGroup.Key} debit total {debits:F2} != credit total {credits:F2}.";
        }

        return null;
    }

    private static string BuildFilePath(string outputDirectory, string branch, string businessDate)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"LIQ_ERPF_GL_{branch}_{businessDate}_{timestamp}.csv";
        return Path.Combine(outputDirectory, fileName);
    }

    private static async Task WriteCsvAsync(
        string filePath,
        IReadOnlyList<GlExtractRow> rows,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new StreamWriter(stream, Encoding.UTF8)
        {
            NewLine = "\r\n",
        };

        await writer.WriteLineAsync(string.Join("|", CsvHeaders));

        foreach (var row in rows)
        {
            await writer.WriteLineAsync(BuildCsvLine(row));
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static string BuildCsvLine(GlExtractRow r) =>
        string.Join("|", new[]
        {
            r.Company,
            r.JournalName,
            r.JournalBatchNumber,
            r.Voucher,
            r.TransDate,
            r.TransEffDate,
            r.Description,
            r.Txt ?? string.Empty,
            r.CurrencyCode,
            r.AmountCurDebit.ToString("F2"),
            r.AmountCurCredit.ToString("F2"),
            r.AmountMSTDebit.ToString("F2"),
            r.AmountMSTCredit.ToString("F2"),
            r.ExchRate.ToString("F6"),
            r.AccountNum,
            r.Dimension1 ?? string.Empty,
            r.Dimension2,
            r.CustAccountNum ?? string.Empty,
            r.CountryRegionId ?? string.Empty,
            r.State ?? string.Empty,
            r.CustGroup,
            r.ReferenceNum ?? string.Empty,
            r.ReferenceNum2 ?? string.Empty,
            r.JournalCategory,
            r.LineNum,
        });
}
