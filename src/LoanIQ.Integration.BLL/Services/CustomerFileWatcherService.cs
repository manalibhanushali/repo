using System.Threading.Channels;
using LoanIQ.Integration.BLL.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanIQ.Integration.BLL.Services;

public sealed class CustomerFileWatcherService(
    IOptions<ConsoleSettings> settings,
    CustomerFileProcessor processor,
    ILogger<CustomerFileWatcherService> logger) : BackgroundService
{
    private readonly Channel<string> _fileQueue = Channel.CreateUnbounded<string>(
        new UnboundedChannelOptions { SingleReader = true });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureDirectoriesExist();

        using var watcher = CreateWatcher();

        logger.LogInformation(
            "Watching directory {Directory} for customer files",
            settings.Value.MonitoredDirectory);

        // Process any files already present when the service starts.
        foreach (var existing in Directory.EnumerateFiles(settings.Value.MonitoredDirectory, "*.json"))
            _fileQueue.Writer.TryWrite(existing);

        await ProcessQueueAsync(stoppingToken);

        _fileQueue.Writer.TryComplete();
    }

    private FileSystemWatcher CreateWatcher()
    {
        var watcher = new FileSystemWatcher(settings.Value.MonitoredDirectory, "*.json")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true,
        };

        watcher.Created += OnFileCreated;
        return watcher;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        logger.LogDebug("File detected: {FilePath}", e.FullPath);
        _fileQueue.Writer.TryWrite(e.FullPath);
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        await foreach (var filePath in _fileQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await WaitForFileReadyAsync(filePath, stoppingToken);
                var result = await processor.ProcessAsync(filePath, stoppingToken);

                if (result.IsSuccess)
                    MoveToProcessed(filePath);
                else
                    logger.LogWarning("File {FilePath} failed processing with {ErrorCount} error(s)", filePath, result.Errors.Count);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing file {FilePath}", filePath);
            }
        }
    }

    // Polls until the file can be opened exclusively, to handle write-lock from the producer.
    private static async Task WaitForFileReadyAsync(string filePath, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return;
            }
            catch (IOException)
            {
                await Task.Delay(200, cancellationToken);
            }
        }
    }

    private void MoveToProcessed(string filePath)
    {
        try
        {
            var destination = Path.Combine(
                settings.Value.ProcessedDirectory,
                Path.GetFileName(filePath));

            if (File.Exists(destination))
                destination = Path.Combine(
                    settings.Value.ProcessedDirectory,
                    $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(filePath)}");

            File.Move(filePath, destination);
            logger.LogInformation("Moved processed file to {Destination}", destination);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not move file {FilePath} to processed directory", filePath);
        }
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(settings.Value.MonitoredDirectory);
        Directory.CreateDirectory(settings.Value.ProcessedDirectory);
    }
}
