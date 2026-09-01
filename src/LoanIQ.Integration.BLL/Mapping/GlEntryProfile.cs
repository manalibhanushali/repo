using AutoMapper;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Mapping;

/// Maps Loan IQ GL entry source data to the ERPF CSV row (ART-020, INTF-06).
/// Implements D/C amount splits, composite description, state/province fallback,
/// external ID stripping, and all default values per the INTF-06 mapping document.
public sealed class GlEntryProfile : Profile
{
    public GlEntryProfile()
    {
        CreateMap<GlEntryRecord, GlExtractRow>()
            .ForMember(d => d.Company,          o => o.MapFrom(s => s.Branch))
            .ForMember(d => d.JournalName,       o => o.MapFrom(_ => "LIQ"))
            .ForMember(d => d.JournalBatchNumber, o => o.MapFrom(_ => "J1"))
            .ForMember(d => d.Voucher,           o => o.MapFrom(_ => "1"))
            .ForMember(d => d.TransDate,         o => o.MapFrom(s => s.PostingDate.ToString("MM/dd/yyyy")))
            .ForMember(d => d.TransEffDate,      o => o.MapFrom(s => s.TransEffDate.ToString("MM/dd/yyyy")))
            .ForMember(d => d.Description,       o => o.MapFrom(s =>
                Truncate($"Trans. Eff. Date ({s.TransEffDate:MM/dd/yyyy}) {s.Description}", 200)))
            .ForMember(d => d.Txt,               o => o.MapFrom(s =>
                s.CustomerFullName != null && s.CustomerExternalId != null
                    ? $"{s.CustomerFullName} / {s.CustomerExternalId}"
                    : s.CustomerFullName))
            .ForMember(d => d.CurrencyCode,      o => o.MapFrom(s => s.CurrencyCode))
            .ForMember(d => d.AmountCurDebit,    o => o.MapFrom(s =>
                s.DebitCreditIndicator == "D" ? Math.Abs(s.TransAmount) : 0m))
            .ForMember(d => d.AmountCurCredit,   o => o.MapFrom(s =>
                s.DebitCreditIndicator == "C" ? Math.Abs(s.TransAmount) : 0m))
            .ForMember(d => d.AmountMSTDebit,    o => o.MapFrom(s =>
                s.DebitCreditIndicator == "D" ? Math.Abs(s.BaseAmount) : 0m))
            .ForMember(d => d.AmountMSTCredit,   o => o.MapFrom(s =>
                s.DebitCreditIndicator == "C" ? Math.Abs(s.BaseAmount) : 0m))
            .ForMember(d => d.ExchRate,          o => o.MapFrom(s => s.FxRate))
            .ForMember(d => d.AccountNum,        o => o.MapFrom(s => s.AccountNum))
            .ForMember(d => d.Dimension1,        o => o.MapFrom(s => s.CostCentre))
            .ForMember(d => d.Dimension2,        o => o.MapFrom(_ => "000"))
            .ForMember(d => d.CustAccountNum,    o => o.MapFrom(s =>
                s.CustomerExternalId != null ? s.CustomerExternalId.TrimStart('0') : null))
            .ForMember(d => d.CountryRegionId,   o => o.MapFrom(s => s.CustomerCountry))
            .ForMember(d => d.State,             o => o.MapFrom(s =>
                !string.IsNullOrWhiteSpace(s.StateCode) ? s.StateCode : s.ProvinceCode))
            .ForMember(d => d.CustGroup,         o => o.MapFrom(_ => "110"))
            .ForMember(d => d.ReferenceNum,      o => o.MapFrom(s => s.FacilityRef))
            .ForMember(d => d.ReferenceNum2,     o => o.MapFrom(s => s.DealRef))
            .ForMember(d => d.JournalCategory,   o => o.MapFrom(s => s.JournalCategory))
            .ForMember(d => d.LineNum,           o => o.MapFrom(s => s.LineNum));
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
