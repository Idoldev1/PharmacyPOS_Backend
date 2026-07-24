namespace POS.API.Models;

public class DrugDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string Form { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int StockQty { get; set; }
    public int ReservedQty { get; set; }
    public int AvailableQty { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public string? NafdacNo { get; set; }
    public string? SupplierId { get; set; }
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public string BranchId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDrugRequest
{
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
    public Guid BrandId { get; set; }
}

public class UpdateDrugRequest
{
    public string Name { get; set; } = null!;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string Form { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public string? NafdacNo { get; set; }
    public string? SupplierId { get; set; }
    public Guid BrandId { get; set; }
}

public class UpdateStockRequest
{
    public int Quantity { get; set; }
}

public class DrugListResult
{
    public List<DrugDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalWorth { get; set; }
}
