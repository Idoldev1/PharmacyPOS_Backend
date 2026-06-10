using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/patients called for branch {BranchId}", branchId);
        var result = await _patientService.GetPatientsAsync(q, page, pageSize, branchId);
        return Ok(result.Payload);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/patients/{PatientId} called", id);
        var result = await _patientService.GetByIdAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("POST /api/patients called for branch {BranchId}", branchId);
        var result = await _patientService.CreateAsync(branchId, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Payload!.Id }, result.Payload);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        _logger.LogInformation("GET /api/patients/{PatientId}/history called", id);
        var result = await _patientService.GetPurchaseHistoryAsync(id);
        return Ok(result.Payload);
    }
}
