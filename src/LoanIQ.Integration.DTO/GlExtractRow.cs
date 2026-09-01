namespace LoanIQ.Integration.DTO;

/// One row in the pipe-delimited GL extract CSV (INTF-06, ART-020).
public sealed record GlExtractRow
{
    public string Company { get; init; } = string.Empty;
    public string JournalName { get; init; } = "LIQ";
    public string JournalBatchNumber { get; init; } = "J1";
    public string Voucher { get; init; } = "1";
    public string TransDate { get; init; } = string.Empty;         // MM/DD/YYYY
    public string TransEffDate { get; init; } = string.Empty;      // MM/DD/YYYY
    public string Description { get; init; } = string.Empty;       // composite, max 200
    public string? Txt { get; init; }                              // CustName / CustId, max 450
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal AmountCurDebit { get; init; }
    public decimal AmountCurCredit { get; init; }
    public decimal AmountMSTDebit { get; init; }
    public decimal AmountMSTCredit { get; init; }
    public decimal ExchRate { get; init; }
    public string AccountNum { get; init; } = string.Empty;
    public string? Dimension1 { get; init; }
    public string Dimension2 { get; init; } = "000";
    public string? CustAccountNum { get; init; }                   // external ID with leading zeros stripped
    public string? CountryRegionId { get; init; }
    public string? State { get; init; }
    public string CustGroup { get; init; } = "110";
    public string? ReferenceNum { get; init; }
    public string? ReferenceNum2 { get; init; }
    public string JournalCategory { get; init; } = string.Empty;
    public string LineNum { get; init; } = string.Empty;
}
