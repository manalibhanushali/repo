namespace LoanIQ.Integration.BLL.Configuration;

public sealed class GlExtractSettings
{
    public const string SectionName = "GlExtractSettings";

    public string InputDirectory { get; init; } = "data/gl-extract/incoming";
    public string ProcessedDirectory { get; init; } = "data/gl-extract/processed";
    public string OutputDirectory { get; init; } = "data/gl-extract/output";

    /// <summary>
    /// Branches for which a zero-row file is always produced even if no entries exist.
    /// </summary>
    public string[] Branches { get; init; } = [];
}
