using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface IPrescriptionService
{
    Task<OperationResult<PrescriptionListResult>> GetPrescriptionsAsync(string? status, string? query, int page, int pageSize, string branchId);
    Task<OperationResult<PrescriptionDto>> GetByIdAsync(Guid id);
    Task<int> CountByStatusAsync(string status, string branchId);
    Task<OperationResult<PrescriptionDto>> CreateAsync(string branchId, CreatePrescriptionRequest request);
    Task<OperationResult> VerifyAsync(Guid id, string userId);
    Task<OperationResult> DispenseAsync(Guid id, string userId);
    Task<OperationResult> FlagAsync(Guid id, string reason);
}
