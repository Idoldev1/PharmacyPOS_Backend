using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IPatientRepository : IRepository<Patient, Guid>
{
    Task<(List<Patient> Items, int Total)> GetPagedAsync(string? query, int page, int pageSize, string branchId);
}
