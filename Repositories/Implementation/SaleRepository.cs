using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class SaleRepository : Repository<Sale, Guid>, ISaleRepository
{
    private readonly ILogger<SaleRepository> _logger;

    public SaleRepository(AppDbContext db, ILogger<SaleRepository> logger) : base(db)
    {
        _logger = logger;
    }

    public override async Task<Sale?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<List<Sale>> GetPagedAsync(int page, int pageSize, string branchId) =>
        await _dbSet.Include(s => s.Items)
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public async Task<List<Sale>> GetTodaySalesAsync(string branchId)
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet.Include(s => s.Items)
            .Where(s => s.BranchId == branchId && s.CreatedAt >= today && s.Status == "completed")
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string branchId) =>
        await _dbSet.CountAsync(s => s.BranchId == branchId);

    public override async Task<Sale> AddAsync(Sale sale)
    {
        await _dbSet.AddAsync(sale);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Sale {ReceiptNo} saved with id {Id}", sale.ReceiptNo, sale.Id);
        return sale;
    }
}
