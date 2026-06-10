using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface ISupplierRepository : IRepository<Supplier, Guid>
{
    Task<List<Supplier>> GetByBranchAsync(string branchId);
    Task<PurchaseOrder?> GetOrderByIdAsync(Guid orderId);
    Task<List<PurchaseOrder>> GetOrdersBySupplierAsync(Guid supplierId);
    Task<PurchaseOrder> AddOrderAsync(PurchaseOrder order);
    Task UpdateOrderAsync(PurchaseOrder order);
}
