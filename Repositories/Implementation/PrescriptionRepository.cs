using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class PrescriptionRepository : Repository<Prescription, Guid>, IPrescriptionRepository
{
    private readonly ILogger<PrescriptionRepository> _logger;

    public PrescriptionRepository(AppDbContext db, ILogger<PrescriptionRepository> logger) : base(db)
    {
        _logger = logger;
    }

    public async Task<(List<Prescription> Items, int Total)> GetPagedAsync(string? status, int page, int pageSize, string branchId)
    {
        var q = _dbSet.Include(p => p.Lines).Where(p => p.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public override async Task<Prescription?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CountByStatusAsync(string status, string branchId) =>
        await _dbSet.CountAsync(p => p.BranchId == branchId && p.Status == status);

    public override async Task<Prescription> AddAsync(Prescription prescription)
    {
        await _dbSet.AddAsync(prescription);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Prescription {RxNumber} saved with id {Id}", prescription.RxNumber, prescription.Id);
        return prescription;
    }
}
