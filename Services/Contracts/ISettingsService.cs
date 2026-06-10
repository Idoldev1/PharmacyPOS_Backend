using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface ISettingsService
{
    Task<OperationResult<BranchSettingsDto>> GetSettingsAsync(string branchId);
    Task<OperationResult<BranchSettingsDto>> UpdateSettingsAsync(string branchId, UpdateSettingsRequest request);
    Task<OperationResult<List<StaffUserDto>>> GetStaffAsync(string branchId);
    Task<OperationResult> ToggleUserActiveAsync(string userId);
}
