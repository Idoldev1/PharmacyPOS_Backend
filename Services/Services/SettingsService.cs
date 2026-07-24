using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS.API.Constants;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repo;
    private readonly AppDbContext _db;
    private readonly UserManager<UserRecord> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(
        ISettingsRepository repo,
        AppDbContext db,
        UserManager<UserRecord> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<SettingsService> logger)
    {
        _repo = repo;
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
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

    public async Task<OperationResult<StaffUserDto>> CreateStaffAsync(string branchId, CreateStaffRequest request)
    {
        _logger.LogInformation("Onboarding staff member {Username}, role {Role}, branch {BranchId}", request.Username, request.Role, branchId);

        var normalizedRole = Roles.Normalize(request.Role);
        if (normalizedRole is null)
        {
            _logger.LogWarning("Staff onboarding rejected for {Username}: invalid role {Role}", request.Username, request.Role);
            return OperationResult<StaffUserDto>.Fail("Invalid role.");
        }

        var user = new UserRecord
        {
            UserName = request.Username,
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            BranchId = branchId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to create account.";
            _logger.LogWarning("Staff onboarding failed for {Username}: {Error}", request.Username, error);
            return OperationResult<StaffUserDto>.Fail(error);
        }

        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            _logger.LogInformation("Creating missing role {Role}", normalizedRole);
            await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
        }

        _logger.LogInformation("Staff member {Username} onboarded, userId {UserId}", request.Username, user.Id);
        return OperationResult<StaffUserDto>.Ok(new StaffUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.UserName!,
            Role = user.Role,
            IsActive = user.IsActive
        });
    }

    public async Task<OperationResult> ResetStaffPasswordAsync(string userId, string newPassword)
    {
        _logger.LogInformation("Admin-initiated password reset for user {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Password reset failed: user not found for userId {UserId}", userId);
            return OperationResult.Fail("User not found.", 404);
        }

        await _userManager.RemovePasswordAsync(user);
        var result = await _userManager.AddPasswordAsync(user, newPassword);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to reset password.";
            _logger.LogWarning("Password reset failed for userId {UserId}: {Error}", userId, error);
            return OperationResult.Fail(error);
        }

        _logger.LogInformation("Password reset successful for userId {UserId}", userId);
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
