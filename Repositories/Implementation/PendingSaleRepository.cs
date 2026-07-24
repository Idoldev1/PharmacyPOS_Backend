using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class PendingSaleRepository : Repository<PendingSale, Guid>, IPendingSaleRepository
{
    public PendingSaleRepository(AppDbContext db) : base(db)
    {
    }

    public override async Task<PendingSale?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PendingSale?> GetByCodeAsync(string code) =>
        await _dbSet.Include(p => p.Items).FirstOrDefaultAsync(p => p.Code == code && p.Status == "Pending");

    public async Task<List<PendingSale>> GetActiveByBranchAsync(string branchId) =>
        await _dbSet.Include(p => p.Items)
            .Where(p => p.BranchId == branchId && p.Status == "Pending")
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<PendingSale>> GetExpiredAsync(DateTime now) =>
        await _dbSet.Include(p => p.Items)
            .Where(p => p.Status == "Pending" && p.ExpiresAt < now)
            .ToListAsync();
}
