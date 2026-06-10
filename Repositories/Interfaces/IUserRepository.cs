using POS.API.Models;

namespace POS.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserRecord?> GetByIdAsync(string id);
    Task<UserRecord?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddUserAsync(UserRecord user);
    Task UpdateUserAsync(UserRecord user);
}
