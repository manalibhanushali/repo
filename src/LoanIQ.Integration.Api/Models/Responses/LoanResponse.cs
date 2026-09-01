namespace LoanIQ.Integration.Api.Models.Responses;

public sealed record LoanResponse(
    Guid Id,
    string BorrowerName,
    string ExternalLoanId,
    decimal PrincipalAmount,
    decimal AnnualInterestRate,
    int TermMonths,
    string Currency,
    LoanStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
