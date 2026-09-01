using Microsoft.Extensions.Logging;

namespace LoanIQ.Integration.BLL.Audit;

internal sealed class NullAuditEventPublisher(ILogger<NullAuditEventPublisher> logger) : IAuditEventPublisher
{
    public Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        logger.LogDebug(
            "Audit event skipped (Service Bus not configured): eventType={EventType}",
            auditEvent.GetType().Name);
        return Task.CompletedTask;
    }
}
