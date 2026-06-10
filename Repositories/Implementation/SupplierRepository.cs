using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.Repositories.Interfaces;

namespace POS.API.Repositories.Implementation;

public class SupplierRepository : Repository<Supplier, Guid>, ISupplierRepository
{
    public SupplierRepository(AppDbContext db) : base(db) { }

    public async Task<List<Supplier>> GetByBranchAsync(string branchId) =>
        await _dbSet.Where(s => s.BranchId == branchId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<PurchaseOrder?> GetOrderByIdAsync(Guid orderId) =>
        await _context.PurchaseOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<List<PurchaseOrder>> GetOrdersBySupplierAsync(Guid supplierId) =>
        await _context.PurchaseOrders
            .Include(o => o.Items)
            .Where(o => o.SupplierId == supplierId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<PurchaseOrder> AddOrderAsync(PurchaseOrder order)
    {
        await _context.PurchaseOrders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateOrderAsync(PurchaseOrder order)
    {
        _context.PurchaseOrders.Update(order);
        await _context.SaveChangesAsync();
    }
}
