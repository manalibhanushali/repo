namespace LoanIQ.Integration.BLL.Configuration;

public sealed class PaymentExtractSettings
{
    public const string SectionName = "PaymentExtractSettings";

    public string InputDirectory { get; init; } = "data/payment-extract/incoming";
    public string ProcessedDirectory { get; init; } = "data/payment-extract/processed";
    public string OutputDirectory { get; init; } = "data/payment-extract/output";
}
