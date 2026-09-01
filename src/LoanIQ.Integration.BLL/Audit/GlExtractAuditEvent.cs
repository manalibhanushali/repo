namespace LoanIQ.Integration.BLL.Audit;

public sealed record GlExtractAuditEvent : AuditEvent
{
    public string Branch { get; init; } = string.Empty;
    public string BusinessDate { get; init; } = string.Empty;
    public string OutputFile { get; init; } = string.Empty;
    public int RecordCount { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
