using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface ISaleRepository : IRepository<Sale, Guid>
{
    Task<List<Sale>> GetPagedAsync(int page, int pageSize, string branchId);
    Task<List<Sale>> GetTodaySalesAsync(string branchId);
    Task<int> GetTotalCountAsync(string branchId);
}
