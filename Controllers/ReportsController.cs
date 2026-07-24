using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.API.Constants;
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

    // Operational summary: Admin, Manager, Chief Pharmacist
    [HttpGet("summary")]
    [RequirePermission(Permissions.Reports.ViewAll, Permissions.Reports.ViewOperational, Permissions.Reports.ViewFinancial)]
    public async Task<IActionResult> Summary()
    {
        _logger.LogInformation("GET /api/reports/summary called for branch {BranchId}", BranchId);
        var r = await _svc.GetSummaryAsync(BranchId);
        return Ok(r.Payload);
    }

    // Revenue trends: Admin, Manager, Cashier (personal sales)
    [HttpGet("weekly-revenue")]
    [RequirePermission(Permissions.Reports.ViewAll, Permissions.Reports.ViewFinancial, Permissions.Reports.ViewPersonalSales)]
    public async Task<IActionResult> WeeklyRevenue()
    {
        _logger.LogInformation("GET /api/reports/weekly-revenue called for branch {BranchId}", BranchId);
        var r = await _svc.GetWeeklyRevenueAsync(BranchId);
        return Ok(r.Payload);
    }

    // Payment breakdown: Admin, Manager
    [HttpGet("payment-breakdown")]
    [RequirePermission(Permissions.Reports.ViewAll, Permissions.Reports.ViewFinancial)]
    public async Task<IActionResult> PaymentBreakdown()
    {
        _logger.LogInformation("GET /api/reports/payment-breakdown called for branch {BranchId}", BranchId);
        var r = await _svc.GetPaymentBreakdownAsync(BranchId);
        return Ok(r.Payload);
    }

    // Top drugs: Admin, Manager, Chief Pharmacist, Pharmacist
    [HttpGet("top-drugs")]
    [RequirePermission(Permissions.Reports.ViewAll, Permissions.Reports.ViewInventory, Permissions.Reports.ViewPrescription)]
    public async Task<IActionResult> TopDrugs()
    {
        _logger.LogInformation("GET /api/reports/top-drugs called for branch {BranchId}", BranchId);
        var r = await _svc.GetTopDrugsAsync(BranchId);
        return Ok(r.Payload);
    }
}
