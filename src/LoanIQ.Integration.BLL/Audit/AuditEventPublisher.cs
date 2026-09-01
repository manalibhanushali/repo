using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LoanIQ.Integration.BLL.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanIQ.Integration.BLL.Audit;

public sealed class AuditEventPublisher : IAuditEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AuditEventPublisher> _logger;

    public AuditEventPublisher(IOptions<AuditSettings> options, ILogger<AuditEventPublisher> logger)
    {
        var settings = options.Value;
        var client = new ServiceBusClient(settings.ConnectionString);
        _sender = client.CreateSender(settings.QueueName);
        _logger = logger;
    }

    public async Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(auditEvent);
        var message = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = $"CustomerAudit.{auditEvent.OperationType}",
        };

        await _sender.SendMessageAsync(message, cancellationToken);
        _logger.LogDebug("Audit event published: eventType={EventType}", auditEvent.GetType().Name);
    }

    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}
