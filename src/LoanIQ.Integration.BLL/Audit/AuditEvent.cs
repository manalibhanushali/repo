namespace LoanIQ.Integration.BLL.Audit;

public abstract record AuditEvent
{
    public DateTimeOffset PublishedAt { get; init; }
    public string OperationType { get; init; } = string.Empty;
}
