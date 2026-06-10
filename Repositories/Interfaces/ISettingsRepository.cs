using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface ISettingsRepository
{
    Task<BranchSettings?> GetByBranchAsync(string branchId);
    Task<BranchSettings> UpsertAsync(BranchSettings settings);
}
