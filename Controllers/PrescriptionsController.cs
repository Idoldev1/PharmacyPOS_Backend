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
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly ILogger<PrescriptionsController> _logger;

    public PrescriptionsController(IPrescriptionService prescriptionService, ILogger<PrescriptionsController> logger)
    {
        _prescriptionService = prescriptionService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission(Permissions.Prescriptions.View)]
    public async Task<IActionResult> GetPrescriptions([FromQuery] string? status, [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/prescriptions called for branch {BranchId}", branchId);
        var result = await _prescriptionService.GetPrescriptionsAsync(status, q, page, pageSize, branchId);
        return Ok(result.Payload);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Prescriptions.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/prescriptions/{RxId} called", id);
        var result = await _prescriptionService.GetByIdAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    [HttpPost]
    [RequirePermission(Permissions.Prescriptions.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("POST /api/prescriptions called for branch {BranchId}", branchId);
        var result = await _prescriptionService.CreateAsync(branchId, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Payload!.Id }, result.Payload);
    }

    // Approve/verify: Chief Pharmacist and Admin only
    [HttpPost("{id:guid}/verify")]
    [RequirePermission(Permissions.Prescriptions.Approve)]
    public async Task<IActionResult> Verify(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("POST /api/prescriptions/{RxId}/verify called by {UserId}", id, userId);
        var result = await _prescriptionService.VerifyAsync(id, userId);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/dispense")]
    [RequirePermission(Permissions.Prescriptions.Dispense)]
    public async Task<IActionResult> Dispense(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("POST /api/prescriptions/{RxId}/dispense called by {UserId}", id, userId);
        var result = await _prescriptionService.DispenseAsync(id, userId);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/flag")]
    [RequirePermission(Permissions.Prescriptions.Create, Permissions.Prescriptions.Approve)]
    public async Task<IActionResult> Flag(Guid id, [FromBody] FlagPrescriptionRequest request)
    {
        _logger.LogInformation("POST /api/prescriptions/{RxId}/flag called", id);
        var result = await _prescriptionService.FlagAsync(id, request.Reason);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }
}
