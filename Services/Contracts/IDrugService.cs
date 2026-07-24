using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface IDrugService
{
    Task<OperationResult<DrugListResult>> GetDrugsAsync(string? query, string? category, Guid? brandId, int page, int pageSize, string branchId);
    Task<OperationResult<List<DrugDto>>> GetLowStockAsync(string branchId);
    Task<OperationResult<DrugDto>> GetByIdAsync(Guid id);
    Task<OperationResult<DrugDto>> CreateAsync(string branchId, CreateDrugRequest request);
    Task<OperationResult<DrugDto>> UpdateAsync(Guid id, UpdateDrugRequest request);
    Task<OperationResult> UpdateStockAsync(Guid id, int quantity);
    Task<OperationResult> DeleteAsync(Guid id);
}
