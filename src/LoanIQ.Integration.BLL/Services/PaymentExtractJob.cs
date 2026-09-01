using System.Text.Json;
using System.Threading.Channels;
using LoanIQ.Integration.BLL.Audit;
using LoanIQ.Integration.BLL.Configuration;
using LoanIQ.Integration.DTO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanIQ.Integration.BLL.Services;

/// Watches for payment extract trigger files and runs the batch XML extraction (INTF-15).
/// Trigger files are JSON documents matching PaymentTriggerFile dropped by the upstream LIQ batch process.
public sealed class PaymentExtractJob(
    IOptions<PaymentExtractSettings> settings,
    PaymentExtractService extractService,
    IAuditEventPublisher auditPublisher,
    ILogger<PaymentExtractJob> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly Channel<string> _fileQueue = Channel.CreateUnbounded<string>(
        new UnboundedChannelOptions { SingleReader = true });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureDirectoriesExist();

        using var watcher = CreateWatcher();

        logger.LogInformation(
            "Payment extract job watching {Directory} for trigger files",
            settings.Value.InputDirectory);

        foreach (var existing in Directory.EnumerateFiles(settings.Value.InputDirectory, "*.json"))
            _fileQueue.Writer.TryWrite(existing);

        await ProcessQueueAsync(stoppingToken);

        _fileQueue.Writer.TryComplete();
    }

    private FileSystemWatcher CreateWatcher()
    {
        var watcher = new FileSystemWatcher(settings.Value.InputDirectory, "*.json")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true,
        };
        watcher.Created += (_, e) => _fileQueue.Writer.TryWrite(e.FullPath);
        return watcher;
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        await foreach (var filePath in _fileQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessTriggerFileAsync(filePath, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing payment trigger file {FilePath}", filePath);
            }
        }
    }

    private async Task ProcessTriggerFileAsync(string filePath, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payment extract trigger file {FilePath}", filePath);

        string json;
        try
        {
            json = await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read trigger file {FilePath}", filePath);
            return;
        }

        PaymentTriggerFile? trigger;
        try
        {
            trigger = JsonSerializer.Deserialize<PaymentTriggerFile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON in trigger file {FilePath}", filePath);
            return;
        }

        if (trigger is null || string.IsNullOrWhiteSpace(trigger.BusinessDate))
        {
            logger.LogError("Trigger file {FilePath} is missing required BusinessDate", filePath);
            return;
        }

        var result = await extractService.ExtractAsync(
            trigger.Payments,
            trigger.BusinessDate,
            settings.Value.OutputDirectory,
            cancellationToken);

        await auditPublisher.PublishAsync(new PaymentExtractAuditEvent
        {
            BusinessDate    = trigger.BusinessDate,
            OutputFile      = result.OutputFile ?? string.Empty,
            RecordCount     = result.ProcessedCount,
            SuppressedCount = result.SuppressedCount,
            Success         = result.IsSuccess,
            ErrorMessage    = result.IsSuccess ? null : string.Join("; ", result.Errors),
            PublishedAt     = DateTimeOffset.UtcNow,
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Payment extract completed with errors for business date {BusinessDate}: {ErrorCount} error(s)",
                trigger.BusinessDate, result.Errors.Count);
        }

        MoveToProcessed(filePath);
    }

    private void MoveToProcessed(string filePath)
    {
        try
        {
            var dest = Path.Combine(
                settings.Value.ProcessedDirectory,
                Path.GetFileName(filePath));

            if (File.Exists(dest))
                dest = Path.Combine(
                    settings.Value.ProcessedDirectory,
                    $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(filePath)}");

            File.Move(filePath, dest);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not move trigger file {FilePath} to processed directory", filePath);
        }
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(settings.Value.InputDirectory);
        Directory.CreateDirectory(settings.Value.ProcessedDirectory);
        Directory.CreateDirectory(settings.Value.OutputDirectory);
    }
}
