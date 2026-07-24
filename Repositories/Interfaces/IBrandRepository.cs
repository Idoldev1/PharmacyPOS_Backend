using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IBrandRepository : IRepository<Brand, Guid>
{
    Task<List<Brand>> GetAllActiveAsync();
    Task<Brand?> GetByNameAsync(string name);
}
