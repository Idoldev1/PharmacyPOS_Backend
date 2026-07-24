using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IDrugRepository : IRepository<Drug, Guid>
{
    Task<(List<Drug> Items, int Total)> GetPagedAsync(string? query, string? category, Guid? brandId, int page, int pageSize, string branchId);
    Task<List<Drug>> GetLowStockAsync(string branchId);
    Task<(decimal TotalValue, decimal TotalWorth)> GetInventoryTotalsAsync(string branchId);
}
