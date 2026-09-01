namespace LoanIQ.Integration.DTO;

public sealed record CustomerLiqRequest
{
    public string OperationType { get; init; } = string.Empty;

    // Core customer fields (INTF-01 CreateCustomer / AmendCustomer)
    public string CustomerStatus { get; init; } = "Active";
    public string Department { get; init; } = "LIQ Operations";
    public string FullName { get; init; } = string.Empty;
    public string? ShortName { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public string? ImmediateParent { get; init; }
    public string? PrimarySic { get; init; }
    public string? CountryCode { get; init; }
    public string? TreasuryReportingAreaCode { get; init; }
    public string CustomerDescription { get; init; } = "DCO";
    public string CraIndicator { get; init; } = "N";
    public string MajorUnderwriterIndicator { get; init; } = "N";
    public string RestrictedIndicator { get; init; } = "N";
    public string SimplifiedCustInd { get; init; } = "N";
    public string BrokerIndicator { get; init; } = "N";
    public string? LenderMeiNumber { get; init; }

    // CustomerCustom fields
    public string? Branch { get; init; }
    public string? ExpenseCode { get; init; }

    // CustomerAlias (zero or one occurrence)
    public string? AliasType { get; init; }
    public string? AliasValue { get; init; }

    // Sub-objects
    public CustomerAddressDto? Address { get; init; }
    public CustomerSettlementDto? Settlement { get; init; }

    // Amend-only
    public string? CustomerRid { get; init; }
}

public sealed record CustomerAddressDto
{
    public string AddressType { get; init; } = "PRIMARY";
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? CountryCode { get; init; }
}

public sealed record CustomerSettlementDto
{
    public string? SettlementCurrency { get; init; }
    public string? SettlementAccountNumber { get; init; }
    public string? SettlementBic { get; init; }
    public string PaymentMethod { get; init; } = "FW";
}
