using FluentValidation;
using LoanIQ.Integration.DTO;

namespace LoanIQ.Integration.BLL.Validation;

/// Enforces INTF-01 inbound field rules for CustomerFileRecord (ART-006).
/// Validation failures cause the record to be logged and skipped — no retry (batch processing policy).
public sealed class CustomerFileRecordValidator : AbstractValidator<CustomerFileRecord>
{
    public CustomerFileRecordValidator()
    {
        RuleFor(x => x.OperationType)
            .NotEmpty()
            .WithMessage("OperationType is required.")
            .Must(v => v.Equals("Create", StringComparison.OrdinalIgnoreCase)
                    || v.Equals("Amend", StringComparison.OrdinalIgnoreCase))
            .WithMessage(x => $"OperationType must be 'Create' or 'Amend'; got '{x.OperationType}'.");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.")
            .MaximumLength(15)
            .WithMessage(x => $"CompanyId exceeds maximum length of 15 characters (length={x.CompanyId.Length}).");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(140)
            .WithMessage(x => $"Name exceeds maximum length of 140 characters (length={x.Name.Length}).");

        RuleFor(x => x.AbbrevName)
            .MaximumLength(30)
            .WithMessage(x => $"AbbrevName exceeds maximum length of 30 characters (length={x.AbbrevName!.Length}).")
            .When(x => x.AbbrevName is not null);

        RuleFor(x => x.LoanIqRid)
            .NotEmpty()
            .WithMessage("LoanIqRid is required for Amend operations.")
            .When(x => x.OperationType.Equals("Amend", StringComparison.OrdinalIgnoreCase));
    }
}
