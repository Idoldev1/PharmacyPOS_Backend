using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.API.Constants;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _svc;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService svc, ILogger<SettingsController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    private string BranchId => User.FindFirstValue("branchId") ?? "hq";

    // Read settings: Admin and Chief Pharmacist (drug catalog)
    [HttpGet]
    [RequirePermission(Permissions.Settings.View, Permissions.Settings.ManageDrugCatalog)]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("GET /api/settings called for branch {BranchId}", BranchId);
        var r = await _svc.GetSettingsAsync(BranchId);
        return Ok(r.Payload);
    }

    // Update system settings: Admin only
    [HttpPut]
    [RequirePermission(Permissions.Settings.ManageSystem)]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request)
    {
        _logger.LogInformation("PUT /api/settings called for branch {BranchId}", BranchId);
        var r = await _svc.UpdateSettingsAsync(BranchId, request);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(r.Payload);
    }

    // View staff list: Admin only
    [HttpGet("staff")]
    [RequirePermission(Permissions.Settings.ManageUsers)]
    public async Task<IActionResult> GetStaff()
    {
        _logger.LogInformation("GET /api/settings/staff called for branch {BranchId}", BranchId);
        var r = await _svc.GetStaffAsync(BranchId);
        return Ok(r.Payload);
    }

    // Toggle user active status: Admin only
    [HttpPatch("staff/{userId}/toggle")]
    [RequirePermission(Permissions.Settings.ManageUsers)]
    public async Task<IActionResult> ToggleUser(string userId)
    {
        _logger.LogInformation("PATCH /api/settings/staff/{UserId}/toggle called", userId);
        var r = await _svc.ToggleUserActiveAsync(userId);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(new { success = true });
    }

    // Onboard a new staff account: Admin only
    [HttpPost("staff")]
    [RequirePermission(Permissions.Settings.ManageUsers)]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        _logger.LogInformation("POST /api/settings/staff called for branch {BranchId}", BranchId);
        var r = await _svc.CreateStaffAsync(BranchId, request);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(r.Payload);
    }

    // Force-set a staff member's password: Admin only
    [HttpPost("staff/{userId}/reset-password")]
    [RequirePermission(Permissions.Settings.ManageUsers)]
    public async Task<IActionResult> ResetStaffPassword(string userId, [FromBody] ResetStaffPasswordRequest request)
    {
        _logger.LogInformation("POST /api/settings/staff/{UserId}/reset-password called", userId);
        var r = await _svc.ResetStaffPasswordAsync(userId, request.NewPassword);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(new { success = true });
    }
}
