using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IPrescriptionRepository : IRepository<Prescription, Guid>
{
    Task<(List<Prescription> Items, int Total)> GetPagedAsync(string? status, int page, int pageSize, string branchId);
    Task<int> CountByStatusAsync(string status, string branchId);
}
