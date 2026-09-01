using AutoMapper;
using FluentAssertions;
using LoanIQ.Integration.BLL.Mapping;
using LoanIQ.Integration.DTO;
using Xunit;

namespace LoanIQ.Integration.BLL.Tests.Mapping;

public sealed class CustomerProfileTests
{
    private readonly IMapper _mapper;

    public CustomerProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CustomerProfile>();
            cfg.AddProfile<AddressProfile>();
            cfg.AddProfile<SettlementInstructionProfile>();
        });
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void CustomerStatus_Defaults_To_Active()
    {
        var source = BuildMinimalRecord();
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.CustomerStatus.Should().Be("Active");
    }

    [Fact]
    public void Department_Defaults_To_LiqOperations()
    {
        var source = BuildMinimalRecord();
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.Department.Should().Be("LIQ Operations");
    }

    [Fact]
    public void ExternalId_Is_ZeroPadded_To_15_Characters()
    {
        var source = BuildMinimalRecord() with { CompanyId = "12345" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.ExternalId.Should().Be("000000000012345");
        result.ExternalId.Should().HaveLength(15);
    }

    [Fact]
    public void ExternalId_Already_15_Characters_Is_Unchanged()
    {
        var source = BuildMinimalRecord() with { CompanyId = "123456789012345" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.ExternalId.Should().Be("123456789012345");
    }

    [Fact]
    public void LenderMeiNumber_Sourced_From_MisCode_When_Available()
    {
        var source = BuildMinimalRecord() with { MisCode = "MEI-ABC-001" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.LenderMeiNumber.Should().Be("MEI-ABC-001");
    }

    [Fact]
    public void LenderMeiNumber_Falls_Back_To_CompanyId_Underscore_ParentCompanyId_When_MisCode_Absent()
    {
        var source = BuildMinimalRecord() with
        {
            CompanyId = "99001",
            ParentCompanyId = "99000",
            MisCode = null,
        };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.LenderMeiNumber.Should().Be("99001_99000");
    }

    [Fact]
    public void LenderMeiNumber_Falls_Back_With_Empty_ParentCompanyId_When_MisCode_Absent()
    {
        var source = BuildMinimalRecord() with
        {
            CompanyId = "99001",
            ParentCompanyId = null,
            MisCode = null,
        };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.LenderMeiNumber.Should().Be("99001_");
    }

    [Fact]
    public void ShortName_Is_Truncated_To_30_Characters()
    {
        var longName = new string('X', 50);
        var source = BuildMinimalRecord() with { AbbrevName = longName };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.ShortName.Should().HaveLength(30);
        result.ShortName.Should().Be(new string('X', 30));
    }

    [Fact]
    public void FullName_Is_Direct_Move_From_Name()
    {
        var source = BuildMinimalRecord() with { Name = "Acme Corporation Ltd" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.FullName.Should().Be("Acme Corporation Ltd");
    }

    [Fact]
    public void ImmediateParent_Is_Null_When_ParentCompanyId_Is_Empty()
    {
        var source = BuildMinimalRecord() with { ParentCompanyId = null };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.ImmediateParent.Should().BeNull();
    }

    [Fact]
    public void ImmediateParent_Is_Set_When_ParentCompanyId_Is_Present()
    {
        var source = BuildMinimalRecord() with { ParentCompanyId = "99000" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.ImmediateParent.Should().Be("99000");
    }

    [Fact]
    public void RestrictedIndicator_Maps_True_To_Y()
    {
        var source = BuildMinimalRecord() with { RestrictedFlag = "true" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.RestrictedIndicator.Should().Be("Y");
    }

    [Fact]
    public void RestrictedIndicator_Maps_False_To_N()
    {
        var source = BuildMinimalRecord() with { RestrictedFlag = "false" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.RestrictedIndicator.Should().Be("N");
    }

    [Fact]
    public void BooleanIndicators_Default_To_N()
    {
        var source = BuildMinimalRecord();
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.CraIndicator.Should().Be("N");
        result.MajorUnderwriterIndicator.Should().Be("N");
        result.SimplifiedCustInd.Should().Be("N");
        result.BrokerIndicator.Should().Be("N");
    }

    [Fact]
    public void TreasuryReportingAreaCode_Is_Derived_From_CountryCode()
    {
        var source = BuildMinimalRecord() with { CountryIso2 = "US" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.TreasuryReportingAreaCode.Should().Be("US");
        result.CountryCode.Should().Be("US");
    }

    [Fact]
    public void ExpenseCode_Is_Derived_From_Branch()
    {
        var source = BuildMinimalRecord() with { Branch = "NYC01" };
        var result = _mapper.Map<CustomerLiqRequest>(source);
        result.ExpenseCode.Should().Be("NYC01");
        result.Branch.Should().Be("NYC01");
    }

    private static CustomerFileRecord BuildMinimalRecord() => new()
    {
        OperationType = "CREATE",
        CompanyId = "1001",
        Name = "Test Company",
    };
}
