using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface ISaleService
{
    Task<OperationResult<SaleDto>> CreateSaleAsync(string cashierId, string branchId, CreateSaleRequest request);
    Task<OperationResult<SaleDto>> GetByIdAsync(Guid id);
    Task<OperationResult<SaleListResult>> GetSalesAsync(int page, int pageSize, string branchId);
    Task<OperationResult<List<SaleDto>>> GetTodaySalesAsync(string branchId);
    Task<OperationResult> RefundAsync(Guid id);
}
