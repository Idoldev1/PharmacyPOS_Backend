using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface ISupplierService
{
    Task<OperationResult<List<SupplierDto>>> GetSuppliersAsync(string branchId);
    Task<OperationResult<SupplierDto>> GetByIdAsync(Guid id);
    Task<OperationResult<SupplierDto>> CreateAsync(string branchId, CreateSupplierRequest request);
    Task<OperationResult<List<PurchaseOrderDto>>> GetOrdersAsync(Guid supplierId);
    Task<OperationResult<PurchaseOrderDto>> CreateOrderAsync(Guid supplierId, string branchId, CreatePurchaseOrderRequest request);
    Task<OperationResult> SendOrderAsync(Guid orderId);
}
