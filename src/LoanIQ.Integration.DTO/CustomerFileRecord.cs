namespace LoanIQ.Integration.DTO;

public sealed record CustomerFileRecord
{
    public string OperationType { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? AbbrevName { get; init; }
    public string? ParentCompanyId { get; init; }
    public string? LoanIqRid { get; init; }
    public string? CountryIso2 { get; init; }
    public string? Branch { get; init; }
    public string? RestrictedFlag { get; init; }
    public string? SicPrimaryId { get; init; }
    public string? AliasValue { get; init; }

    public CustomerAddressRecord? Address { get; init; }
    public CustomerSettlementRecord? Settlement { get; init; }

    // Populated by the service layer from the MISCode table before mapping.
    public string? MisCode { get; init; }
}

public sealed record CustomerAddressRecord
{
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? StateCode { get; init; }
    public string? ProvinceCode { get; init; }
    public string? PostCode { get; init; }
    public string? CountryIso2 { get; init; }
}

public sealed record CustomerSettlementRecord
{
    public string? BaseCurrencyIso { get; init; }
    public string? AccountNumber { get; init; }
    public string? SwiftBic { get; init; }
}
