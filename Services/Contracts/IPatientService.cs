using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface IPatientService
{
    Task<OperationResult<PatientListResult>> GetPatientsAsync(string? query, int page, int pageSize, string branchId);
    Task<OperationResult<PatientDto>> GetByIdAsync(Guid id);
    Task<OperationResult<PatientDto>> CreateAsync(string branchId, CreatePatientRequest request);
    Task<OperationResult<List<SaleDto>>> GetPurchaseHistoryAsync(Guid patientId);
}
