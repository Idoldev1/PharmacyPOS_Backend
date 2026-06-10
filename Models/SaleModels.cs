namespace POS.API.Models;

public class CreateSaleRequest
{
    public List<CreateSaleItem> Items { get; set; } = [];
    public string PaymentMethod { get; set; } = null!;
    public decimal Discount { get; set; } = 0;
    public string? PatientId { get; set; }
}

public class CreateSaleItem
{
    public Guid DrugId { get; set; }
    public int Quantity { get; set; }
}

public class SaleDto
{
    public Guid Id { get; set; }
    public string ReceiptNo { get; set; } = null!;
    public string? PatientId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string CashierId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<SaleItemDto> Items { get; set; } = [];
}

public class SaleItemDto
{
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class SaleListResult
{
    public List<SaleDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
