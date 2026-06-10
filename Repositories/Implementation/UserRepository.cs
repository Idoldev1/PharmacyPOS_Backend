using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext dbContext, ILogger<UserRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogDebug("UserRepository initialized");
    }

    public async Task<UserRecord?> GetByIdAsync(string id)
    {
        _logger.LogDebug("Retrieving user by id {UserId}", id);
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id);
        _logger.LogDebug("User by id {UserId}: {Found}", id, user is not null ? "found" : "not found");
        return user;
    }

    public async Task<UserRecord?> GetByUsernameAsync(string username)
    {
        _logger.LogDebug("Retrieving user by username {Username}", username);
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName!.ToLower() == username.ToLower());
        _logger.LogDebug("User by username {Username}: {Found}", username, user is not null ? "found" : "not found");
        return user;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        _logger.LogDebug("Checking existence for username {Username}", username);
        var exists = await _dbContext.Users.AnyAsync(u => u.UserName!.ToLower() == username.ToLower());
        _logger.LogDebug("User existence for username {Username}: {Exists}", username, exists);
        return exists;
    }

    public async Task AddUserAsync(UserRecord user)
    {
        _logger.LogInformation("Adding user with username {Username}, userId {UserId}", user.UserName, user.Id);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User added successfully with username {Username}, userId {UserId}", user.UserName, user.Id);
    }

    public async Task UpdateUserAsync(UserRecord user)
    {
        _logger.LogInformation("Updating user with userId {UserId}", user.Id);
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("User updated successfully for userId {UserId}", user.Id);
    }
}
