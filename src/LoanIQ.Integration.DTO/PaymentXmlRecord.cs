using System.Xml.Serialization;

namespace LoanIQ.Integration.DTO;

/// Root element of the TWS payment extract XML file (INTF-15, schema v0002).
[XmlRoot("File")]
public sealed class PaymentFile
{
    [XmlElement("File_Type")]
    public string FileType { get; set; } = "AP PAYMENTS";

    [XmlElement("File_Header_Record")]
    public PaymentFileHeader Header { get; set; } = new();

    [XmlElement("Payment_Record")]
    public List<PaymentXmlRecord> Payments { get; set; } = [];

    [XmlElement("File_Trailer_Record")]
    public PaymentFileTrailer Trailer { get; set; } = new();
}

public sealed class PaymentFileHeader
{
    [XmlElement("File_Format_Version")]
    public string FileFormatVersion { get; set; } = "0002";

    [XmlElement("Creation_Module")]
    public string CreationModule { get; set; } = "LIQ_SSS";

    [XmlElement("Creation_Date")]
    public string CreationDate { get; set; } = string.Empty;   // YYYYMMDD

    [XmlElement("Record_Count")]
    public string RecordCount { get; set; } = "000000";        // zero-padded to 6
}

public sealed class PaymentXmlRecord
{
    [XmlElement("Payment_Reference")]
    public string PaymentReference { get; set; } = string.Empty;

    [XmlElement("Payment_Method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [XmlElement("Value_Date")]
    public string ValueDate { get; set; } = string.Empty;      // YYYYMMDD

    [XmlElement("Payment_Amount")]
    public string PaymentAmount { get; set; } = string.Empty;  // absolute value, 2 dp

    [XmlElement("Payment_Currency")]
    public string PaymentCurrency { get; set; } = string.Empty;

    [XmlElement("Debit_Account")]
    public string DebitAccount { get; set; } = string.Empty;

    [XmlElement("Debit_Branch")]
    public string DebitBranch { get; set; } = string.Empty;

    [XmlElement("Beneficiary")]
    public BeneficiaryXml? Beneficiary { get; set; }

    [XmlElement("FW_Block")]
    public FwBlockXml? FwBlock { get; set; }

    [XmlElement("ACH_Block")]
    public AchBlockXml? AchBlock { get; set; }

    [XmlElement("IMT_Block")]
    public ImtBlockXml? ImtBlock { get; set; }

    [XmlElement("Remittance")]
    public RemittanceXml? Remittance { get; set; }

    [XmlElement("GL_Link")]
    public string? GlLink { get; set; }

    public bool ShouldSerializeGlLink() => GlLink != null;
}

public sealed class BeneficiaryXml
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Account")]
    public string? Account { get; set; }

    public bool ShouldSerializeAccount() => Account != null;

    [XmlElement("Country")]
    public string? Country { get; set; }

    public bool ShouldSerializeCountry() => Country != null;
}

public sealed class FwBlockXml
{
    [XmlElement("Routing_Number")]
    public string RoutingNumber { get; set; } = string.Empty;

    [XmlElement("Bank_Name")]
    public string? BankName { get; set; }

    public bool ShouldSerializeBankName() => BankName != null;

    [XmlElement("Charge_Bearer")]
    public string ChargeBearer { get; set; } = "OUR";
}

public sealed class AchBlockXml
{
    [XmlElement("Routing_Number")]
    public string RoutingNumber { get; set; } = string.Empty;

    [XmlElement("Entry_Class")]
    public string EntryClass { get; set; } = "CCD";

    [XmlElement("Settlement_Days")]
    public string SettlementDays { get; set; } = "2";
}

public sealed class ImtBlockXml
{
    [XmlElement("BIC")]
    public string Bic { get; set; } = string.Empty;

    [XmlElement("IBAN")]
    public string? Iban { get; set; }

    public bool ShouldSerializeIban() => Iban != null;

    [XmlElement("Intermediary_BIC")]
    public string? IntermediaryBic { get; set; }

    public bool ShouldSerializeIntermediaryBic() => IntermediaryBic != null;

    [XmlElement("Charge_Bearer")]
    public string ChargeBearer { get; set; } = string.Empty;
}

public sealed class RemittanceXml
{
    [XmlElement("Info")]
    public string? Info { get; set; }

    public bool ShouldSerializeInfo() => Info != null;

    [XmlElement("Facility_Ref")]
    public string? FacilityRef { get; set; }

    public bool ShouldSerializeFacilityRef() => FacilityRef != null;
}

public sealed class PaymentFileTrailer
{
    [XmlElement("Hash")]
    public string Hash { get; set; } = string.Empty;
}
