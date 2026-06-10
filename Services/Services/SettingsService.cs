using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;
    private readonly AppDbContext _db;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository repo, AppDbContext db, ILogger<SettingsService> logger)
    {
        _repo = repo;
        _db = db;
        _logger = logger;
    }

    public async Task<OperationResult<BranchSettingsDto>> GetSettingsAsync(string branchId)
    {
        _logger.LogInformation("Fetching settings for branch {BranchId}", branchId);
        var settings = await _repo.GetByBranchAsync(branchId)
            ?? new BranchSettings { BranchId = branchId };
        return OperationResult<BranchSettingsDto>.Ok(ToDto(settings));
    }

    public async Task<OperationResult<BranchSettingsDto>> UpdateSettingsAsync(string branchId, UpdateSettingsRequest request)
    {
        _logger.LogInformation("Updating settings for branch {BranchId}", branchId);
        var settings = new BranchSettings
        {
            BranchId = branchId,
            PharmacyName = request.PharmacyName,
            BranchName = request.BranchName,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            NafdacNumber = request.NafdacNumber,
            ReceiptHeader = request.ReceiptHeader,
            ReceiptFooter = request.ReceiptFooter,
            ShowLogo = request.ShowLogo,
            ShowBarcode = request.ShowBarcode,
            VatRate = request.VatRate,
            TaxId = request.TaxId,
            ApplyVat = request.ApplyVat,
            AutoBackup = request.AutoBackup
        };
        var saved = await _repo.UpsertAsync(settings);
        _logger.LogInformation("Settings updated for branch {BranchId}, pharmacy {PharmacyName}", branchId, saved.PharmacyName);
        return OperationResult<BranchSettingsDto>.Ok(ToDto(saved));
    }

    public async Task<OperationResult<List<StaffUserDto>>> GetStaffAsync(string branchId)
    {
        _logger.LogInformation("Fetching staff list for branch {BranchId}", branchId);
        var users = await _db.Users
            .Where(u => u.BranchId == branchId)
            .OrderBy(u => u.LastName)
            .ToListAsync();

        _logger.LogInformation("Found {Count} staff member(s) for branch {BranchId}", users.Count, branchId);
        return OperationResult<List<StaffUserDto>>.Ok(users.Select(u => new StaffUserDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Username = u.UserName ?? "",
            Role = u.Role,
            IsActive = u.IsActive
        }).ToList());
    }

    public async Task<OperationResult> ToggleUserActiveAsync(string userId)
    {
        _logger.LogInformation("Toggling active status for user {UserId}", userId);
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when toggling active status", userId);
            return OperationResult.Fail("User not found.", 404);
        }
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        _logger.LogInformation("User {UserId} active status set to {IsActive}", userId, user.IsActive);
        return OperationResult.Ok();
    }

    private static BranchSettingsDto ToDto(BranchSettings s) => new()
    {
        PharmacyName = s.PharmacyName,
        BranchName = s.BranchName,
        Address = s.Address,
        Phone = s.Phone,
        Email = s.Email,
        NafdacNumber = s.NafdacNumber,
        ReceiptHeader = s.ReceiptHeader,
        ReceiptFooter = s.ReceiptFooter,
        ShowLogo = s.ShowLogo,
        ShowBarcode = s.ShowBarcode,
        VatRate = s.VatRate,
        TaxId = s.TaxId,
        ApplyVat = s.ApplyVat,
        AutoBackup = s.AutoBackup,
        LastBackupAt = s.LastBackupAt
    };
}
