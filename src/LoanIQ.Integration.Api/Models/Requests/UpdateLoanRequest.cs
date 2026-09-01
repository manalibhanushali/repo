using System.ComponentModel.DataAnnotations;

namespace LoanIQ.Integration.Api.Models.Requests;

public sealed record UpdateLoanRequest(
    [Required, StringLength(100)] string BorrowerName,
    [Range(0.001, 100.0)] decimal AnnualInterestRate,
    LoanStatus Status
);
