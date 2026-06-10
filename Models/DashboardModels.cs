namespace POS.API.Models;

public class DashboardSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public int TodayTransactions { get; set; }
    public int LowStockCount { get; set; }
    public int PendingRxCount { get; set; }
    public List<DrugDto> LowStockDrugs { get; set; } = [];
}
