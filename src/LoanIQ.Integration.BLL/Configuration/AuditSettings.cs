namespace LoanIQ.Integration.BLL.Configuration;

public sealed class AuditSettings
{
    public const string SectionName = "AuditSettings";

    public string ConnectionString { get; init; } = string.Empty;
    public string QueueName { get; init; } = "customer-audit";
}
