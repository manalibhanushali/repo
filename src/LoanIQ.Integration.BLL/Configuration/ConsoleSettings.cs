namespace LoanIQ.Integration.BLL.Configuration;

public sealed class ConsoleSettings
{
    public const string SectionName = "ConsoleSettings";

    public string MonitoredDirectory { get; init; } = string.Empty;
    public string ProcessedDirectory { get; init; } = string.Empty;
}
