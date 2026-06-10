namespace POS.API.Models;

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Address { get; set; }
}

public class PurchaseOrderItemRequest
{
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDelivery { get; set; }
    public List<PurchaseOrderItemRequest> Items { get; set; } = [];
}

public class PurchaseOrderItemDto
{
    public Guid Id { get; set; }
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Subtotal { get; set; }
}

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string PoNumber { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDelivery { get; set; }
    public string Status { get; set; } = null!;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = [];
}
