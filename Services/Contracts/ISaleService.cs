using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface ISaleService
{
    Task<OperationResult<PendingSaleDto>> InitiateSaleAsync(string pharmacistId, string branchId, InitiateSaleRequest request);
    Task<OperationResult<PendingSaleDto>> GetPendingSaleByCodeAsync(string code);
    Task<OperationResult<List<PendingSaleDto>>> GetActivePendingSalesAsync(string branchId);
    Task<OperationResult<SaleDto>> CompleteSaleAsync(string cashierId, string code, CompleteSaleRequest request);
    Task<OperationResult> CancelPendingSaleAsync(Guid id);
    Task ExpireDueSalesAsync();

    Task<OperationResult<SaleDto>> GetByIdAsync(Guid id);
    Task<OperationResult<SaleListResult>> GetSalesAsync(int page, int pageSize, string branchId);
    Task<OperationResult<List<SaleDto>>> GetTodaySalesAsync(string branchId);
    Task<OperationResult> RefundAsync(Guid id);
}
