namespace LoanIQ.Integration.BLL.Audit;

public sealed record PaymentExtractAuditEvent : AuditEvent
{
    public string BusinessDate { get; init; } = string.Empty;
    public string OutputFile { get; init; } = string.Empty;
    public int RecordCount { get; init; }
    public int SuppressedCount { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
