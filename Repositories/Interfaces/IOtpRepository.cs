using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IOtpRepository
{
    Task<OtpEntry?> GetByUserIdAsync(string userId);
    Task AddAsync(OtpEntry entry);
    Task DeleteByUserIdAsync(string userId);
}
