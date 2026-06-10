namespace POS.API.Models;

public class Sale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReceiptNo { get; set; } = null!;
    public string? PatientId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string CashierId { get; set; } = null!;
    public string BranchId { get; set; } = null!;
    public string Status { get; set; } = "completed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SaleItem> Items { get; set; } = [];
}

public class SaleItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
