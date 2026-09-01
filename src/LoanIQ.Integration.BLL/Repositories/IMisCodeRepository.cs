namespace LoanIQ.Integration.BLL.Repositories;

public interface IMisCodeRepository
{
    Task<string?> GetMisCodeAsync(string companyId, CancellationToken cancellationToken = default);
}
