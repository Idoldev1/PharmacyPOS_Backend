using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _svc;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportsService svc, ILogger<ReportsController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    private string BranchId => User.FindFirstValue("branchId") ?? "hq";

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        _logger.LogInformation("GET /api/reports/summary called for branch {BranchId}", BranchId);
        var r = await _svc.GetSummaryAsync(BranchId);
        return Ok(r.Payload);
    }

    [HttpGet("weekly-revenue")]
    public async Task<IActionResult> WeeklyRevenue()
    {
        _logger.LogInformation("GET /api/reports/weekly-revenue called for branch {BranchId}", BranchId);
        var r = await _svc.GetWeeklyRevenueAsync(BranchId);
        return Ok(r.Payload);
    }

    [HttpGet("payment-breakdown")]
    public async Task<IActionResult> PaymentBreakdown()
    {
        _logger.LogInformation("GET /api/reports/payment-breakdown called for branch {BranchId}", BranchId);
        var r = await _svc.GetPaymentBreakdownAsync(BranchId);
        return Ok(r.Payload);
    }

    [HttpGet("top-drugs")]
    public async Task<IActionResult> TopDrugs()
    {
        _logger.LogInformation("GET /api/reports/top-drugs called for branch {BranchId}", BranchId);
        var r = await _svc.GetTopDrugsAsync(BranchId);
        return Ok(r.Payload);
    }
}
