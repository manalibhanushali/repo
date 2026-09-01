namespace LoanIQ.Integration.DTO;

/// Source data record from Loan IQ payment and settlement tables (INTF-15).
public sealed record PaymentRecord
{
    // TLS_PAYMENT fields
    public string PaymentReference { get; init; } = string.Empty;  // PAY_PID_PAYMENT (15 chars), idempotency key
    public string PaymentMethod { get; init; } = string.Empty;     // PAY_CDE_METHOD: FW, ACH, or IMT
    public DateOnly ValueDate { get; init; }                        // PAY_DTE_VALUE
    public decimal PaymentAmount { get; init; }                     // PAY_AMT_PAYMENT (absolute value)
    public string PaymentCurrency { get; init; } = string.Empty;   // PAY_CDE_CURRENCY (ISO 4217)

    // TLS_BANK_ACCOUNT fields (Meridian internal debit account)
    public string DebitAccount { get; init; } = string.Empty;      // BNK_TXT_ACCOUNT_NO (34 chars)
    public string DebitBranch { get; init; } = string.Empty;       // BNK_CDE_BRANCH (5 chars) — suppression key

    // Beneficiary data (TLS_CUSTOMER, TLS_CUST_SI, TLS_CUST_ADDRESS)
    public string BeneficiaryName { get; init; } = string.Empty;   // CUS_NME_FULL_NAME (truncated to 35 in output)
    public string? BeneficiaryAccount { get; init; }               // SI_TXT_ACCOUNT_NO
    public string? BeneficiaryCountry { get; init; }               // ADR_CDE_COUNTRY

    // Credit branch — used for suppression rule (internal transfer detection)
    public string? CreditBranch { get; init; }

    // FW block (TLS_CUST_SI)
    public string? FwRoutingNumber { get; init; }                  // SI_TXT_ABA (9 chars)
    public string? FwBankName { get; init; }                       // SI_NME_BANK (truncated to 35 in output)

    // ACH block (TLS_CUST_SI)
    public string? AchRoutingNumber { get; init; }                 // SI_TXT_ABA (9 chars)

    // IMT block (TLS_CUST_SI)
    public string? ImtBic { get; init; }                           // SI_TXT_BIC (11 chars)
    public string? ImtIban { get; init; }                          // SI_TXT_IBAN (34 chars)
    public string? ImtIntermediaryBic { get; init; }               // SI_TXT_INT_BIC — omit if null
    public string? ImtChargeBearer { get; init; }                  // SI_CDE_CHARGE: OUR, BEN, SHA

    // Remittance (TLS_PAYMENT, TLS_FACILITY)
    public string? FacilityName { get; init; }                     // FAC_NME_FACILITY
    public string? PaymentNarrative { get; init; }                 // PAY_TXT_NARRATIVE
    public string? FacilityReference { get; init; }                // FAC_PID_FACILITY (15 chars)

    // GL link (TLS_GL_ENTRY)
    public string? GlLinkReference { get; init; }                  // GLE_PID_ENTRY (15 chars)
}

/// Wrapper for the payment extract trigger file dropped by the upstream LIQ batch process.
public sealed record PaymentTriggerFile
{
    public string BusinessDate { get; init; } = string.Empty;     // YYYYMMDD
    public IReadOnlyList<PaymentRecord> Payments { get; init; } = [];
}
