using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;

namespace POS.API.Repositories.Implementation;

public class PatientRepository : Repository<Patient, Guid>, IPatientRepository
{
    public PatientRepository(AppDbContext db) : base(db) { }

    public async Task<(List<Patient> Items, int Total)> GetPagedAsync(string? query, int page, int pageSize, string branchId)
    {
        var q = _dbSet.Where(p => p.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p =>
                p.FirstName.Contains(query) ||
                p.LastName.Contains(query) ||
                p.Phone.Contains(query));
        var total = await q.CountAsync();
        var items = await q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }
}
