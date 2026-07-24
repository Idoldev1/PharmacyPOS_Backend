using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class ReportsService : IReportsService
{
    private readonly AppDbContext _db;
    private readonly IDrugRepository _drugRepo;
    private readonly ILogger<ReportsService> _logger;

    public ReportsService(AppDbContext db, IDrugRepository drugRepo, ILogger<ReportsService> logger)
    {
        _db = db;
        _drugRepo = drugRepo;
        _logger = logger;
    }

    public async Task<OperationResult<ReportsSummary>> GetSummaryAsync(string branchId)
    {
        _logger.LogInformation("Fetching reports summary for branch {BranchId}", branchId);
        var today = DateTime.UtcNow.Date;
        var todaySales = await _db.Sales
            .Where(s => s.BranchId == branchId && s.CreatedAt >= today && s.Status == "Completed")
            .ToListAsync();

        var todayRevenue = todaySales.Sum(s => s.Total);
        var todayCount = todaySales.Count;
        var avgSale = todayCount > 0 ? todayRevenue / todayCount : 0;

        var lowStock = await _db.Drugs
            .CountAsync(d => d.BranchId == branchId && d.IsActive && d.StockQty <= d.ReorderLevel);

        // Top selling drug this month
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var topDrug = await _db.SaleItems
            .Join(_db.Sales, i => i.SaleId, s => s.Id, (i, s) => new { i, s })
            .Where(x => x.s.BranchId == branchId && x.s.CreatedAt >= monthStart && x.s.Status == "Completed")
            .GroupBy(x => new { x.i.DrugName, x.i.BrandName })
            .Select(g => new { g.Key.DrugName, g.Key.BrandName, Units = g.Sum(x => x.i.Quantity) })
            .OrderByDescending(x => x.Units)
            .FirstOrDefaultAsync();

        _logger.LogInformation("Reports summary for branch {BranchId}: revenue ₦{Revenue}, transactions {Count}, low-stock {LowStock}",
            branchId, todayRevenue, todayCount, lowStock);

        return OperationResult<ReportsSummary>.Ok(new ReportsSummary
        {
            TodayRevenue = todayRevenue,
            TodayTransactions = todayCount,
            AvgSaleValue = avgSale,
            LowStockCount = lowStock,
            TopSellingDrug = topDrug?.DrugName ?? "—",
            TopSellingBrand = topDrug?.BrandName,
            TopSellingUnits = topDrug?.Units ?? 0
        });
    }

    public async Task<OperationResult<List<WeeklyRevenuePoint>>> GetWeeklyRevenueAsync(string branchId)
    {
        _logger.LogInformation("Fetching weekly revenue for branch {BranchId}", branchId);
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1);
        var points = new List<WeeklyRevenuePoint>();

        for (int i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            var next = day.AddDays(1);
            var rev = await _db.Sales
                .Where(s => s.BranchId == branchId && s.CreatedAt >= day && s.CreatedAt < next && s.Status == "Completed")
                .SumAsync(s => (decimal?)s.Total) ?? 0;
            points.Add(new WeeklyRevenuePoint
            {
                Day = day.ToString("ddd"),
                Revenue = rev
            });
        }

        return OperationResult<List<WeeklyRevenuePoint>>.Ok(points);
    }

    public async Task<OperationResult<List<PaymentMethodPoint>>> GetPaymentBreakdownAsync(string branchId)
    {
        _logger.LogInformation("Fetching payment breakdown for branch {BranchId}", branchId);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var groups = await _db.Sales
            .Where(s => s.BranchId == branchId && s.CreatedAt >= monthStart && s.Status == "Completed")
            .GroupBy(s => s.PaymentMethod)
            .Select(g => new { Method = g.Key, Count = g.Count() })
            .ToListAsync();

        var total = groups.Sum(g => g.Count);
        var colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["cash"] = "#1A7F5A",
            ["card"] = "#3B82F6",
            ["pos"] = "#3B82F6",
            ["transfer"] = "#F59E0B",
            ["hmo"] = "#8B5CF6"
        };

        var result = groups.Select(g => new PaymentMethodPoint
        {
            Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(g.Method.ToLower()),
            Value = total > 0 ? (int)Math.Round((double)g.Count / total * 100) : 0,
            Color = colors.GetValueOrDefault(g.Method.ToLower(), "#94A3B8")
        }).ToList();

        return OperationResult<List<PaymentMethodPoint>>.Ok(result);
    }

    public async Task<OperationResult<List<TopDrugEntry>>> GetTopDrugsAsync(string branchId)
    {
        _logger.LogInformation("Fetching top drugs for branch {BranchId}", branchId);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var top = await _db.SaleItems
            .Join(_db.Sales, i => i.SaleId, s => s.Id, (i, s) => new { i, s })
            .Where(x => x.s.BranchId == branchId && x.s.CreatedAt >= monthStart && x.s.Status == "Completed")
            .GroupBy(x => new { x.i.DrugName, x.i.BrandName })
            .Select(g => new TopDrugEntry
            {
                Name = g.Key.DrugName,
                Brand = g.Key.BrandName,
                Units = g.Sum(x => x.i.Quantity),
                Revenue = g.Sum(x => x.i.Subtotal)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync();

        return OperationResult<List<TopDrugEntry>>.Ok(top);
    }
}
