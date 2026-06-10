using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDrugService _drugService;
    private readonly ISaleService _saleService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDrugService drugService, ISaleService saleService, IPrescriptionService prescriptionService, ILogger<DashboardController> logger)
    {
        _drugService = drugService;
        _saleService = saleService;
        _prescriptionService = prescriptionService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var branchId = User.FindFirstValue("branchId") ?? "hq";
        _logger.LogInformation("GET /api/dashboard/summary called for branch {BranchId}", branchId);

        var todaySalesResult = await _saleService.GetTodaySalesAsync(branchId);
        var lowStockResult = await _drugService.GetLowStockAsync(branchId);
        var pendingRxCount = await _prescriptionService.CountByStatusAsync("pending", branchId);

        var todaySales = todaySalesResult.Payload ?? [];
        var lowStockDrugs = lowStockResult.Payload ?? [];

        return Ok(new DashboardSummaryDto
        {
            TodayRevenue = todaySales.Sum(s => s.Total),
            TodayTransactions = todaySales.Count,
            LowStockCount = lowStockDrugs.Count,
            PendingRxCount = pendingRxCount,
            LowStockDrugs = lowStockDrugs
        });
    }
}
