using AutoMapper;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Mapping;

/// Maps PMS CompanyBank to Loan IQ SettlementInstruction DTO (ART-005).
/// Shared between INTF-01 (customer create) and INTF-15 (payment beneficiary details).
public sealed class SettlementInstructionProfile : Profile
{
    public SettlementInstructionProfile()
    {
        CreateMap<CustomerSettlementRecord, CustomerSettlementDto>()
            .ForMember(d => d.SettlementCurrency,       o => o.MapFrom(s => s.BaseCurrencyIso))
            .ForMember(d => d.SettlementAccountNumber,  o => o.MapFrom(s => s.AccountNumber))
            .ForMember(d => d.SettlementBic,            o => o.MapFrom(s => s.SwiftBic))
            .ForMember(d => d.PaymentMethod,            o => o.MapFrom(_ => "FW"));
    }
}
