using AutoMapper;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Mapping;

/// Maps PMS CompanyAddress to Loan IQ Address DTO.
/// Implements the state/province fallback rule: use StateCode if populated, else ProvinceCode (CR-102, ART-004).
public sealed class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<CustomerAddressRecord, CustomerAddressDto>()
            .ForMember(d => d.AddressType,  o => o.MapFrom(_ => "PRIMARY"))
            .ForMember(d => d.AddressLine1, o => o.MapFrom(s => s.AddressLine1))
            .ForMember(d => d.AddressLine2, o => o.MapFrom(s => s.AddressLine2))
            .ForMember(d => d.City,         o => o.MapFrom(s => s.City))
            .ForMember(d => d.State,        o => o.MapFrom(s =>
                !string.IsNullOrWhiteSpace(s.StateCode) ? s.StateCode : s.ProvinceCode))
            .ForMember(d => d.PostalCode,   o => o.MapFrom(s => s.PostCode))
            .ForMember(d => d.CountryCode,  o => o.MapFrom(s => s.CountryIso2));
    }
}
