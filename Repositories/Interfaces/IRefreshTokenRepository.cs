using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenEntry?> GetByTokenAsync(string token);
    Task AddAsync(RefreshTokenEntry entry);
    Task DeleteAsync(string token);
    Task DeleteByUserIdAsync(string userId);
}
