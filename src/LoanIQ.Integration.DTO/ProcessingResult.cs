namespace LoanIQ.Integration.DTO;

public sealed record ProcessingResult
{
    public bool IsSuccess { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static ProcessingResult Success() =>
        new() { IsSuccess = true };

    public static ProcessingResult Failure(IReadOnlyList<string> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public static ProcessingResult Failure(string error) =>
        Failure([error]);
}
