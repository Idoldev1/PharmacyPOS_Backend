using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(ISaleService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSales([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/sales called for branch {BranchId}", branchId);
        var result = await _saleService.GetSalesAsync(page, pageSize, branchId);
        return Ok(result.Payload);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/sales/{SaleId} called", id);
        var result = await _saleService.GetByIdAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("POST /api/sales called by cashier {UserId}", userId);
        var result = await _saleService.CreateSaleAsync(userId, branchId, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Payload!.Id }, result.Payload);
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id)
    {
        _logger.LogInformation("POST /api/sales/{SaleId}/refund called", id);
        var result = await _saleService.RefundAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }
}
