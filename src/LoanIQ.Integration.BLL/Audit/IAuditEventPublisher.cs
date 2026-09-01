namespace LoanIQ.Integration.BLL.Audit;

public interface IAuditEventPublisher
{
    Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
