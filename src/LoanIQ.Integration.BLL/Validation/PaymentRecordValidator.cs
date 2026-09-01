using FluentValidation;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Validation;

/// Enforces INTF-15 inbound field rules for payment records (ART-040).
/// Invalid records are logged and skipped; suppression is applied upstream before validation.
public sealed class PaymentRecordValidator : AbstractValidator<PaymentRecord>
{
    private static readonly string[] ValidMethods = ["FW", "ACH", "IMT"];

    public PaymentRecordValidator()
    {
        RuleFor(x => x.PaymentReference)
            .NotEmpty().WithMessage("PaymentReference is required.")
            .MaximumLength(15).WithMessage(x => $"PaymentReference exceeds 15 characters (length={x.PaymentReference.Length}).");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("PaymentMethod is required.")
            .Must(m => ValidMethods.Contains(m, StringComparer.OrdinalIgnoreCase))
            .WithMessage(x => $"PaymentMethod must be FW, ACH, or IMT; got '{x.PaymentMethod}'.");

        RuleFor(x => x.PaymentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("PaymentAmount must be non-negative.");

        RuleFor(x => x.PaymentCurrency)
            .NotEmpty().WithMessage("PaymentCurrency is required.")
            .MaximumLength(3).WithMessage(x => $"PaymentCurrency exceeds 3 characters (length={x.PaymentCurrency.Length}).");

        RuleFor(x => x.DebitAccount)
            .NotEmpty().WithMessage("DebitAccount is required.")
            .MaximumLength(34).WithMessage(x => $"DebitAccount exceeds 34 characters (length={x.DebitAccount.Length}).");

        RuleFor(x => x.DebitBranch)
            .NotEmpty().WithMessage("DebitBranch is required.")
            .MaximumLength(5).WithMessage(x => $"DebitBranch exceeds 5 characters (length={x.DebitBranch.Length}).");

        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("BeneficiaryName is required.")
            .MaximumLength(140).WithMessage(x => $"BeneficiaryName exceeds 140 characters (length={x.BeneficiaryName.Length}).");

        // Method-specific mandatory fields
        RuleFor(x => x.FwRoutingNumber)
            .NotEmpty().WithMessage("FwRoutingNumber is required when PaymentMethod is FW.")
            .MaximumLength(9).WithMessage("FwRoutingNumber exceeds 9 characters.")
            .When(x => x.PaymentMethod.Equals("FW", StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.AchRoutingNumber)
            .NotEmpty().WithMessage("AchRoutingNumber is required when PaymentMethod is ACH.")
            .MaximumLength(9).WithMessage("AchRoutingNumber exceeds 9 characters.")
            .When(x => x.PaymentMethod.Equals("ACH", StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.ImtBic)
            .NotEmpty().WithMessage("ImtBic is required when PaymentMethod is IMT.")
            .MaximumLength(11).WithMessage("ImtBic exceeds 11 characters.")
            .When(x => x.PaymentMethod.Equals("IMT", StringComparison.OrdinalIgnoreCase));
    }
}
