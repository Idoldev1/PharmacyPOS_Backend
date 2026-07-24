using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;

namespace POS.API.Repositories.Implementation;

public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _db;

    public OtpRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<OtpEntry?> GetByUserIdAsync(string userId) =>
        _db.OtpEntries.FirstOrDefaultAsync(x => x.UserId == userId);

    public async Task AddAsync(OtpEntry entry)
    {
        _db.OtpEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        var entries = _db.OtpEntries.Where(x => x.UserId == userId);
        _db.OtpEntries.RemoveRange(entries);
        await _db.SaveChangesAsync();
    }
}
