using AutoMapper;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Mapping;

/// Maps PMS CompanyDto to Loan IQ LiqCustomerDto payload (ART-003, INTF-01).
/// Implements direct moves, defaults, derived fields, and conditional omissions
/// per the INTF-01 mapping document.
public sealed class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<CustomerFileRecord, CustomerLiqRequest>()
            .ForMember(d => d.OperationType,             o => o.MapFrom(s => s.OperationType))
            .ForMember(d => d.CustomerStatus,            o => o.MapFrom(_ => "Active"))
            .ForMember(d => d.Department,                o => o.MapFrom(_ => "LIQ Operations"))
            .ForMember(d => d.FullName,                  o => o.MapFrom(s => s.Name))
            .ForMember(d => d.ShortName,                 o => o.MapFrom(s =>
                s.AbbrevName == null ? null
                : s.AbbrevName.Length > 30 ? s.AbbrevName.Substring(0, 30)
                : s.AbbrevName))
            .ForMember(d => d.ExternalId,                o => o.MapFrom(s => s.CompanyId.PadLeft(15, '0')))
            .ForMember(d => d.ImmediateParent,           o => o.MapFrom(s =>
                string.IsNullOrWhiteSpace(s.ParentCompanyId) ? null : s.ParentCompanyId))
            .ForMember(d => d.PrimarySic,                o => o.MapFrom(s => s.SicPrimaryId))
            .ForMember(d => d.CountryCode,               o => o.MapFrom(s => s.CountryIso2))
            .ForMember(d => d.TreasuryReportingAreaCode, o => o.MapFrom(s => s.CountryIso2))
            .ForMember(d => d.CustomerDescription,       o => o.MapFrom(_ => "DCO"))
            .ForMember(d => d.CraIndicator,              o => o.MapFrom(_ => "N"))
            .ForMember(d => d.MajorUnderwriterIndicator, o => o.MapFrom(_ => "N"))
            .ForMember(d => d.RestrictedIndicator,       o => o.MapFrom(s =>
                "true".Equals(s.RestrictedFlag, StringComparison.OrdinalIgnoreCase) ? "Y" : "N"))
            .ForMember(d => d.SimplifiedCustInd,         o => o.MapFrom(_ => "N"))
            .ForMember(d => d.BrokerIndicator,           o => o.MapFrom(_ => "N"))
            .ForMember(d => d.Branch,                    o => o.MapFrom(s => s.Branch))
            .ForMember(d => d.ExpenseCode,               o => o.MapFrom(s => s.Branch))
            .ForMember(d => d.AliasType,                 o => o.MapFrom(s =>
                s.AliasValue != null ? "LEGACY" : null))
            .ForMember(d => d.AliasValue,                o => o.MapFrom(s => s.AliasValue))
            .ForMember(d => d.LenderMeiNumber,           o => o.MapFrom(s =>
                !string.IsNullOrEmpty(s.MisCode)
                    ? s.MisCode
                    : s.CompanyId + "_" + (s.ParentCompanyId ?? "")))
            .ForMember(d => d.Address,                   o => o.MapFrom(s => s.Address))
            .ForMember(d => d.Settlement,                o => o.MapFrom(s => s.Settlement))
            .ForMember(d => d.CustomerRid,               o => o.MapFrom(s => s.LoanIqRid));
    }
}
