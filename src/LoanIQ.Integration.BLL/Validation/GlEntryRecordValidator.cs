using FluentValidation;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Validation;

/// Enforces INTF-06 inbound field rules for GL entry records (ART-020).
/// Invalid entries are logged and skipped; voucher imbalance aborts the branch extraction.
public sealed class GlEntryRecordValidator : AbstractValidator<GlEntryRecord>
{
    public GlEntryRecordValidator()
    {
        RuleFor(x => x.Branch)
            .NotEmpty().WithMessage("Branch is required.")
            .MaximumLength(5).WithMessage(x => $"Branch exceeds 5 characters (length={x.Branch.Length}).");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("CurrencyCode is required.")
            .MaximumLength(3).WithMessage(x => $"CurrencyCode exceeds 3 characters (length={x.CurrencyCode.Length}).");

        RuleFor(x => x.DebitCreditIndicator)
            .NotEmpty().WithMessage("DebitCreditIndicator is required.")
            .Must(v => v == "D" || v == "C")
            .WithMessage(x => $"DebitCreditIndicator must be 'D' or 'C'; got '{x.DebitCreditIndicator}'.");

        RuleFor(x => x.JournalCategory)
            .NotEmpty().WithMessage("JournalCategory is required.")
            .MaximumLength(20).WithMessage(x => $"JournalCategory exceeds 20 characters (length={x.JournalCategory.Length}).");

        RuleFor(x => x.LineNum)
            .NotEmpty().WithMessage("LineNum is required.")
            .MaximumLength(15).WithMessage(x => $"LineNum exceeds 15 characters (length={x.LineNum.Length}).");

        RuleFor(x => x.Voucher)
            .NotEmpty().WithMessage("Voucher is required.");

        RuleFor(x => x.AccountNum)
            .NotEmpty().WithMessage("AccountNum is required.")
            .MaximumLength(10).WithMessage(x => $"AccountNum exceeds 10 characters (length={x.AccountNum.Length}).");

        RuleFor(x => x.TransAmount)
            .GreaterThanOrEqualTo(0).WithMessage("TransAmount must be non-negative.");

        RuleFor(x => x.BaseAmount)
            .GreaterThanOrEqualTo(0).WithMessage("BaseAmount must be non-negative.");
    }
}
