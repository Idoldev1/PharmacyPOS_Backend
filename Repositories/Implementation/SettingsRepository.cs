using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;

namespace POS.API.Repositories.Implementation;

public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;

    public SettingsRepository(AppDbContext db) => _db = db;

    public async Task<BranchSettings?> GetByBranchAsync(string branchId) =>
        await _db.BranchSettings.FirstOrDefaultAsync(s => s.BranchId == branchId);

    public async Task<BranchSettings> UpsertAsync(BranchSettings settings)
    {
        var existing = await GetByBranchAsync(settings.BranchId);
        if (existing is null)
        {
            await _db.BranchSettings.AddAsync(settings);
        }
        else
        {
            existing.PharmacyName = settings.PharmacyName;
            existing.BranchName = settings.BranchName;
            existing.Address = settings.Address;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.NafdacNumber = settings.NafdacNumber;
            existing.ReceiptHeader = settings.ReceiptHeader;
            existing.ReceiptFooter = settings.ReceiptFooter;
            existing.ShowLogo = settings.ShowLogo;
            existing.ShowBarcode = settings.ShowBarcode;
            existing.VatRate = settings.VatRate;
            existing.TaxId = settings.TaxId;
            existing.ApplyVat = settings.ApplyVat;
            existing.AutoBackup = settings.AutoBackup;
            existing.UpdatedAt = DateTime.UtcNow;
            settings = existing;
        }
        await _db.SaveChangesAsync();
        return settings;
    }
}
