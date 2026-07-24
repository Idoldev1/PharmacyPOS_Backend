using POS.API.Models;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(ISupplierRepository repo, ILogger<SupplierService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<OperationResult<List<SupplierDto>>> GetSuppliersAsync(string branchId)
    {
        _logger.LogInformation("Fetching suppliers for branch {BranchId}", branchId);
        var suppliers = await _repo.GetByBranchAsync(branchId);
        _logger.LogInformation("Found {Count} supplier(s) for branch {BranchId}", suppliers.Count, branchId);
        return OperationResult<List<SupplierDto>>.Ok(suppliers.Select(ToDto).ToList());
    }

    public async Task<OperationResult<SupplierDto>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching supplier {SupplierId}", id);
        var s = await _repo.GetByIdAsync(id);
        if (s is null)
        {
            _logger.LogWarning("Supplier {SupplierId} not found", id);
            return OperationResult<SupplierDto>.Fail("Supplier not found.", 404);
        }
        return OperationResult<SupplierDto>.Ok(ToDto(s));
    }

    public async Task<OperationResult<SupplierDto>> CreateAsync(string branchId, CreateSupplierRequest request)
    {
        _logger.LogInformation("Creating supplier {SupplierName} for branch {BranchId}", request.Name, branchId);
        var supplier = new Supplier
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            BranchId = branchId
        };
        var saved = await _repo.AddAsync(supplier);
        _logger.LogInformation("Supplier {SupplierName} created with id {SupplierId}", saved.Name, saved.Id);
        return OperationResult<SupplierDto>.Ok(ToDto(saved));
    }

    public async Task<OperationResult<List<PurchaseOrderDto>>> GetOrdersAsync(Guid supplierId)
    {
        _logger.LogInformation("Fetching purchase orders for supplier {SupplierId}", supplierId);
        var orders = await _repo.GetOrdersBySupplierAsync(supplierId);
        _logger.LogInformation("Found {Count} order(s) for supplier {SupplierId}", orders.Count, supplierId);
        return OperationResult<List<PurchaseOrderDto>>.Ok(orders.Select(ToOrderDto).ToList());
    }

    public async Task<OperationResult<PurchaseOrderDto>> CreateOrderAsync(Guid supplierId, string branchId, CreatePurchaseOrderRequest request)
    {
        _logger.LogInformation("Creating purchase order for supplier {SupplierId}, branch {BranchId}, {ItemCount} item(s)", supplierId, branchId, request.Items.Count);

        if (request.Items.Count == 0)
            return OperationResult<PurchaseOrderDto>.Fail("Order must have at least one item.");

        var supplier = await _repo.GetByIdAsync(supplierId);
        if (supplier is null)
        {
            _logger.LogWarning("Supplier {SupplierId} not found when creating order", supplierId);
            return OperationResult<PurchaseOrderDto>.Fail("Supplier not found.", 404);
        }

        var items = request.Items.Select(i => new PurchaseOrderItem
        {
            DrugId = i.DrugId,
            DrugName = i.DrugName,
            BrandName = i.BrandName,
            Quantity = i.Quantity,
            UnitCost = i.UnitCost,
            Subtotal = i.UnitCost * i.Quantity
        }).ToList();

        var order = new PurchaseOrder
        {
            SupplierId = supplierId,
            PoNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            OrderDate = request.OrderDate,
            ExpectedDelivery = request.ExpectedDelivery,
            Status = "draft",
            BranchId = branchId,
            Total = items.Sum(i => i.Subtotal),
            Items = items
        };

        var saved = await _repo.AddOrderAsync(order);
        _logger.LogInformation("Purchase order {PoNumber} created with id {OrderId}, total ₦{Total}", saved.PoNumber, saved.Id, saved.Total);
        return OperationResult<PurchaseOrderDto>.Ok(ToOrderDto(saved));
    }

    public async Task<OperationResult> SendOrderAsync(Guid orderId)
    {
        _logger.LogInformation("Sending purchase order {OrderId}", orderId);
        var order = await _repo.GetOrderByIdAsync(orderId);
        if (order is null)
        {
            _logger.LogWarning("Purchase order {OrderId} not found", orderId);
            return OperationResult.Fail("Order not found.", 404);
        }
        if (order.Status == "sent")
        {
            _logger.LogWarning("Purchase order {OrderId} already sent", orderId);
            return OperationResult.Fail("Order already sent.");
        }

        order.Status = "sent";
        await _repo.UpdateOrderAsync(order);
        _logger.LogInformation("Purchase order {PoNumber} ({OrderId}) marked as sent", order.PoNumber, orderId);
        return OperationResult.Ok();
    }

    private static SupplierDto ToDto(Supplier s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        ContactPerson = s.ContactPerson,
        Phone = s.Phone,
        Email = s.Email,
        Address = s.Address,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };

    private static PurchaseOrderDto ToOrderDto(PurchaseOrder o) => new()
    {
        Id = o.Id,
        SupplierId = o.SupplierId,
        PoNumber = o.PoNumber,
        OrderDate = o.OrderDate,
        ExpectedDelivery = o.ExpectedDelivery,
        Status = o.Status,
        Total = o.Total,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new PurchaseOrderItemDto
        {
            Id = i.Id,
            DrugId = i.DrugId,
            DrugName = i.DrugName,
            BrandName = i.BrandName,
            Quantity = i.Quantity,
            UnitCost = i.UnitCost,
            Subtotal = i.Subtotal
        }).ToList()
    };
}
