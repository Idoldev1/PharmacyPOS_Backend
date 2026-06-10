using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IPasswordResetRepository
{
    Task<PasswordResetEntry?> GetByTokenAsync(string token);
    Task AddAsync(PasswordResetEntry entry);
    Task DeleteAsync(string token);
}
