using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("GET /api/settings called for branch {BranchId}", BranchId);
        var r = await _svc.GetSettingsAsync(BranchId);
        return Ok(r.Payload);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request)
    {
        _logger.LogInformation("PUT /api/settings called for branch {BranchId}", BranchId);
        var r = await _svc.UpdateSettingsAsync(BranchId, request);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(r.Payload);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaff()
    {
        _logger.LogInformation("GET /api/settings/staff called for branch {BranchId}", BranchId);
        var r = await _svc.GetStaffAsync(BranchId);
        return Ok(r.Payload);
    }

    [HttpPatch("staff/{userId}/toggle")]
    public async Task<IActionResult> ToggleUser(string userId)
    {
        _logger.LogInformation("PATCH /api/settings/staff/{UserId}/toggle called", userId);
        var r = await _svc.ToggleUserActiveAsync(userId);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(new { success = true });
    }
}
