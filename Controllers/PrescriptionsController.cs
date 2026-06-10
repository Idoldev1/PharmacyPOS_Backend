using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetPrescriptions([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/prescriptions called for branch {BranchId}", branchId);
        var result = await _prescriptionService.GetPrescriptionsAsync(status, page, pageSize, branchId);
        return Ok(result.Payload);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/prescriptions/{RxId} called", id);
        var result = await _prescriptionService.GetByIdAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("POST /api/prescriptions called for branch {BranchId}", branchId);
        var result = await _prescriptionService.CreateAsync(branchId, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Payload!.Id }, result.Payload);
    }

    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("POST /api/prescriptions/{RxId}/verify called by {UserId}", id, userId);
        var result = await _prescriptionService.VerifyAsync(id, userId);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/dispense")]
    public async Task<IActionResult> Dispense(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("POST /api/prescriptions/{RxId}/dispense called by {UserId}", id, userId);
        var result = await _prescriptionService.DispenseAsync(id, userId);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }

    [HttpPost("{id:guid}/flag")]
    public async Task<IActionResult> Flag(Guid id, [FromBody] FlagPrescriptionRequest request)
    {
        _logger.LogInformation("POST /api/prescriptions/{RxId}/flag called", id);
        var result = await _prescriptionService.FlagAsync(id, request.Reason);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }
}
