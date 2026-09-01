using AutoMapper;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Mapping;

/// Maps Loan IQ PaymentRecord to TWS XML payment record (ART-040, INTF-15).
/// Applies mutually exclusive method blocks, truncations, suppression is handled by the service.
public sealed class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentRecord, PaymentXmlRecord>()
            .ForMember(d => d.PaymentReference, o => o.MapFrom(s => s.PaymentReference))
            .ForMember(d => d.PaymentMethod,    o => o.MapFrom(s => s.PaymentMethod))
            .ForMember(d => d.ValueDate,         o => o.MapFrom(s => s.ValueDate.ToString("yyyyMMdd")))
            .ForMember(d => d.PaymentAmount,     o => o.MapFrom(s => Math.Abs(s.PaymentAmount).ToString("F2")))
            .ForMember(d => d.PaymentCurrency,   o => o.MapFrom(s => s.PaymentCurrency))
            .ForMember(d => d.DebitAccount,      o => o.MapFrom(s => s.DebitAccount))
            .ForMember(d => d.DebitBranch,       o => o.MapFrom(s => s.DebitBranch))
            .ForMember(d => d.Beneficiary,       o => o.MapFrom(s => new BeneficiaryXml
            {
                Name    = Truncate(s.BeneficiaryName, 35),
                Account = s.BeneficiaryAccount,
                Country = s.BeneficiaryCountry,
            }))
            .ForMember(d => d.FwBlock,           o => o.MapFrom(s =>
                s.PaymentMethod == "FW"
                    ? new FwBlockXml
                    {
                        RoutingNumber = s.FwRoutingNumber ?? string.Empty,
                        BankName      = s.FwBankName != null ? Truncate(s.FwBankName, 35) : null,
                        ChargeBearer  = "OUR",
                    }
                    : null))
            .ForMember(d => d.AchBlock,          o => o.MapFrom(s =>
                s.PaymentMethod == "ACH"
                    ? new AchBlockXml
                    {
                        RoutingNumber  = s.AchRoutingNumber ?? string.Empty,
                        EntryClass     = "CCD",
                        SettlementDays = "2",
                    }
                    : null))
            .ForMember(d => d.ImtBlock,          o => o.MapFrom(s =>
                s.PaymentMethod == "IMT"
                    ? new ImtBlockXml
                    {
                        Bic             = s.ImtBic ?? string.Empty,
                        Iban            = s.ImtIban,
                        IntermediaryBic = s.ImtIntermediaryBic,
                        ChargeBearer    = MapChargeBearer(s.ImtChargeBearer),
                    }
                    : null))
            .ForMember(d => d.Remittance, o => o.MapFrom(s => BuildRemittance(s)))
            .ForMember(d => d.GlLink, o => o.MapFrom(s => s.GlLinkReference));
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static string MapChargeBearer(string? value) => value?.ToUpperInvariant() switch
    {
        "OUR" => "OUR",
        "BEN" => "BEN",
        "SHA" => "SHA",
        _     => value ?? string.Empty,
    };

    private static RemittanceXml? BuildRemittance(PaymentRecord s)
    {
        var info = BuildRemittanceInfo(s.FacilityName, s.PaymentNarrative);
        if (info == null && s.FacilityReference == null) return null;
        return new RemittanceXml
        {
            Info        = info,
            FacilityRef = s.FacilityReference,
        };
    }

    private static string? BuildRemittanceInfo(string? facilityName, string? narrative)
    {
        if (facilityName == null && narrative == null) return null;
        var combined = $"{facilityName} {narrative}".Trim();
        return Truncate(combined, 140);
    }
}
