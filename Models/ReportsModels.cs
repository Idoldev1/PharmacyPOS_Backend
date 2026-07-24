namespace POS.API.Models;

public class ReportsSummary
{
    public decimal TodayRevenue { get; set; }
    public int TodayTransactions { get; set; }
    public decimal AvgSaleValue { get; set; }
    public int LowStockCount { get; set; }
    public string TopSellingDrug { get; set; } = "—";
    public string? TopSellingBrand { get; set; }
    public int TopSellingUnits { get; set; }
}

public class WeeklyRevenuePoint
{
    public string Day { get; set; } = null!;
    public decimal Revenue { get; set; }
}

public class PaymentMethodPoint
{
    public string Name { get; set; } = null!;
    public int Value { get; set; }
    public string Color { get; set; } = null!;
}

public class TopDrugEntry
{
    public string Name { get; set; } = null!;
    public string? Brand { get; set; }
    public int Units { get; set; }
    public decimal Revenue { get; set; }
}
