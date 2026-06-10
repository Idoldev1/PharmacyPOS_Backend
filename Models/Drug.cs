namespace POS.API.Models;

public class Drug
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string Form { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int StockQty { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public string? NafdacNo { get; set; }
    public string? SupplierId { get; set; }
    public string BranchId { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
