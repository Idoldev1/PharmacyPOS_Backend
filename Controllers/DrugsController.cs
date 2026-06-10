using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DrugsController : ControllerBase
{
    private readonly IDrugService _drugService;
    private readonly ILogger<DrugsController> _logger;

    public DrugsController(IDrugService drugService, ILogger<DrugsController> logger)
    {
        _drugService = drugService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDrugs([FromQuery] string? query, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/drugs called for branch {BranchId}", branchId);
        var result = await _drugService.GetDrugsAsync(query, category, page, pageSize, branchId);
        return Ok(result.Payload);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/drugs/low-stock called for branch {BranchId}", branchId);
        var result = await _drugService.GetLowStockAsync(branchId);
        return Ok(result.Payload);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/drugs/{DrugId} called", id);
        var result = await _drugService.GetByIdAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDrugRequest request)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("POST /api/drugs called for branch {BranchId}", branchId);
        var result = await _drugService.CreateAsync(branchId, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Payload!.Id }, result.Payload);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        _logger.LogInformation("PATCH /api/drugs/{DrugId}/stock called", id);
        var result = await _drugService.UpdateStockAsync(id, request.Quantity);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("DELETE /api/drugs/{DrugId} called", id);
        var result = await _drugService.DeleteAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }
}
