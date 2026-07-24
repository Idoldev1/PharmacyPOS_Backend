using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IPendingSaleRepository : IRepository<PendingSale, Guid>
{
    Task<PendingSale?> GetByCodeAsync(string code);
    Task<List<PendingSale>> GetActiveByBranchAsync(string branchId);
    Task<List<PendingSale>> GetExpiredAsync(DateTime now);
}
