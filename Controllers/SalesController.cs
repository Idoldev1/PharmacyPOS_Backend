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
    [RequirePermission(Permissions.Sales.View)]
    public async Task<IActionResult> GetSales([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/sales called for branch {BranchId}", branchId);
        var result = await _saleService.GetSalesAsync(page, pageSize, branchId);
        return Ok(result.Payload);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Sales.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/sales/{SaleId} called", id);
        var result = await _saleService.GetByIdAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    // Pharmacist/Chief Pharmacist: reserve stock and generate a hand-off code.
    [HttpPost("pending")]
    [RequirePermission(Permissions.Sales.Create)]
    public async Task<IActionResult> InitiateSale([FromBody] InitiateSaleRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("POST /api/sales/pending called by user {UserId}", userId);
        var result = await _saleService.InitiateSaleAsync(userId, branchId, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    // Pharmacist/Chief Pharmacist: list this branch's active pending sales (for cancelling).
    [HttpGet("pending")]
    [RequirePermission(Permissions.Sales.Cancel)]
    public async Task<IActionResult> GetActivePendingSales()
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        var result = await _saleService.GetActivePendingSalesAsync(branchId);
        return Ok(result.Payload);
    }

    // Cashier/Manager/Admin: look up a pending sale by its code before completing it.
    [HttpGet("pending/{code}")]
    [RequirePermission(Permissions.Sales.Complete)]
    public async Task<IActionResult> GetPendingSaleByCode(string code)
    {
        var result = await _saleService.GetPendingSaleByCodeAsync(code);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }

    // Cashier/Manager/Admin: finalize the sale — deducts stock, records payment.
    [HttpPost("pending/{code}/complete")]
    [RequirePermission(Permissions.Sales.Complete)]
    public async Task<IActionResult> CompleteSale(string code, [FromBody] CompleteSaleRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _logger.LogInformation("POST /api/sales/pending/{Code}/complete called by user {UserId}", code, userId);
        var result = await _saleService.CompleteSaleAsync(userId, code, request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Payload!.Id }, result.Payload);
    }

    // Pharmacist/Chief Pharmacist: cancel a pending sale, releasing its reservation.
    [HttpPost("pending/{id:guid}/cancel")]
    [RequirePermission(Permissions.Sales.Cancel)]
    public async Task<IActionResult> CancelPendingSale(Guid id)
    {
        _logger.LogInformation("POST /api/sales/pending/{Id}/cancel called", id);
        var result = await _saleService.CancelPendingSaleAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }

    // Void/refund: Manager and Admin only
    [HttpPost("{id:guid}/refund")]
    [RequirePermission(Permissions.Sales.Void, Permissions.Billing.Refund)]
    public async Task<IActionResult> Refund(Guid id)
    {
        _logger.LogInformation("POST /api/sales/{SaleId}/refund called", id);
        var result = await _saleService.RefundAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(new { success = true });
    }
}
