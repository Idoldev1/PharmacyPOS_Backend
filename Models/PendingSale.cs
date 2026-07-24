namespace POS.API.Models;

public class PendingSale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = null!;
    public string? PatientId { get; set; }
    public string InitiatedByUserId { get; set; } = null!;
    public string BranchId { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public List<PendingSaleItem> Items { get; set; } = [];
}

public class PendingSaleItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PendingSaleId { get; set; }
    public PendingSale PendingSale { get; set; } = null!;
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = null!;
    public string? BrandName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
