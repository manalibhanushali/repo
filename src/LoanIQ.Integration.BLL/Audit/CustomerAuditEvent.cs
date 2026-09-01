namespace LoanIQ.Integration.BLL.Audit;

public sealed record CustomerAuditEvent : AuditEvent
{
    public string ExternalId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public bool Success { get; init; }
}
