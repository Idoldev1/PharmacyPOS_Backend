using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;

namespace POS.API.Repositories.Implementation;

public class BrandRepository : Repository<Brand, Guid>, IBrandRepository
{
    public BrandRepository(AppDbContext db) : base(db) { }

    public async Task<List<Brand>> GetAllActiveAsync() =>
        await _dbSet.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();

    public async Task<Brand?> GetByNameAsync(string name) =>
        await _dbSet.FirstOrDefaultAsync(b => b.Name == name);
}
