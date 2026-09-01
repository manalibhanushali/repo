using System.ComponentModel.DataAnnotations;

namespace LoanIQ.Integration.Api.Models.Requests;

public sealed record CreateLoanRequest(
    [Required, StringLength(100)] string BorrowerName,
    [Required, StringLength(20)] string ExternalLoanId,
    [Range(0.01, double.MaxValue)] decimal PrincipalAmount,
    [Range(0.001, 100.0)] decimal AnnualInterestRate,
    [Range(1, 600)] int TermMonths,
    [Required] string Currency
);
