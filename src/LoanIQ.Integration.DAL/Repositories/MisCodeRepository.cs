using LoanIQ.Integration.BLL.Repositories;

namespace LoanIQ.Integration.DAL.Repositories;

// Stub implementation — returns null (no MISCode found) until the DAL team
// wires up the Oracle/Dapper query against the MISCODE table.
public sealed class MisCodeRepository : IMisCodeRepository
{
    public Task<string?> GetMisCodeAsync(string companyId, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);
}
