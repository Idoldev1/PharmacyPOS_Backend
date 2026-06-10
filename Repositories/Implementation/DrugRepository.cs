using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class DrugRepository : Repository<Drug, Guid>, IDrugRepository
{
    private readonly ILogger<DrugRepository> _logger;

    public DrugRepository(AppDbContext db, ILogger<DrugRepository> logger) : base(db)
    {
        _logger = logger;
    }

    public async Task<(List<Drug> Items, int Total)> GetPagedAsync(string? query, string? category, int page, int pageSize, string branchId)
    {
        var q = _dbSet.Where(d => d.IsActive && d.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(d => d.Name.Contains(query) || (d.GenericName != null && d.GenericName.Contains(query)));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(d => d.Category == category);
        var total = await q.CountAsync();
        var items = await q.OrderBy(d => d.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        _logger.LogDebug("GetPagedAsync returned {Count}/{Total} drugs for branch {BranchId}", items.Count, total, branchId);
        return (items, total);
    }

    public async Task<List<Drug>> GetLowStockAsync(string branchId) =>
        await _dbSet.Where(d => d.IsActive && d.BranchId == branchId && d.StockQty <= d.ReorderLevel)
            .OrderBy(d => d.StockQty).ToListAsync();

    public override async Task<Drug> AddAsync(Drug drug)
    {
        await _dbSet.AddAsync(drug);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Drug {Name} added with id {Id}", drug.Name, drug.Id);
        return drug;
    }
}
