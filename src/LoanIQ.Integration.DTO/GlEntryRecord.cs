namespace LoanIQ.Integration.DTO;

/// Source data record from Loan IQ TLS_GL_ENTRY and related tables.
/// Only PENDING status entries are extracted (selection criteria on GLE_STA_STATUS).
public sealed record GlEntryRecord
{
    // TLS_GL_ENTRY fields
    public string Branch { get; init; } = string.Empty;           // GLE_CDE_BRANCH (5 chars)
    public DateOnly PostingDate { get; init; }                    // GLE_DTE_POSTING
    public DateOnly TransEffDate { get; init; }                   // GLE_DTE_TRANS_EFF
    public string Description { get; init; } = string.Empty;     // GLE_TXT_DESCRIPTION / narrative text
    public string CurrencyCode { get; init; } = string.Empty;    // GLE_CDE_CURRENCY (ISO 4217)
    public decimal TransAmount { get; init; }                    // GLE_AMT_TRANS (absolute value)
    public string DebitCreditIndicator { get; init; } = string.Empty; // GLE_IND_DR_CR: 'D' or 'C'
    public decimal BaseAmount { get; init; }                     // GLE_AMT_BASE (absolute value, USD)
    public decimal FxRate { get; init; } = 1m;                   // GLE_RTE_FX (6 decimal places)
    public string Voucher { get; init; } = string.Empty;         // voucher grouping key

    // TLS_GL_ACCOUNT fields
    public string AccountNum { get; init; } = string.Empty;      // GLA_CDE_ACCOUNT (10 chars)
    public string? CostCentre { get; init; }                     // GLA_CDE_COST_CENTRE (6 chars)

    // TLS_CUSTOMER fields
    public string? CustomerFullName { get; init; }               // CUS_NME_FULL_NAME
    public string? CustomerExternalId { get; init; }             // CUS_XID_CUST_ID (15 chars, zero-padded)
    public string? CustomerCountry { get; init; }                // CUS_CDE_COUNTRY

    // TLS_CUST_ADDRESS fields
    public string? StateCode { get; init; }                      // ADR_CDE_STATE (2 chars)
    public string? ProvinceCode { get; init; }                   // ADR_CDE_PROVINCE (5 chars)

    // TLS_FACILITY / TLS_DEAL fields
    public string? FacilityRef { get; init; }                    // FAC_PID_FACILITY (15 chars)
    public string? DealRef { get; init; }                        // DEA_PID_DEAL (15 chars)

    // Looked-up via GL_EVENT_XREF
    public string JournalCategory { get; init; } = string.Empty; // GLE_CDE_EVENT_TYPE mapped value (20 chars)

    // Idempotency key
    public string LineNum { get; init; } = string.Empty;         // GLE_PID_ENTRY (15 chars)
}

/// Wrapper for the GL extract trigger file dropped by the upstream batch process.
public sealed record GlEntryTriggerFile
{
    public string BusinessDate { get; init; } = string.Empty;   // YYYYMMDD
    public IReadOnlyList<GlEntryRecord> Entries { get; init; } = [];
}
