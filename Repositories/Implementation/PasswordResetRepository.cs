using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PasswordResetRepository> _logger;

    public PasswordResetRepository(AppDbContext dbContext, ILogger<PasswordResetRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogDebug("PasswordResetRepository initialized");
    }

    public async Task<PasswordResetEntry?> GetByTokenAsync(string token)
    {
        _logger.LogDebug("Retrieving password reset token entry");
        var entry = await _dbContext.PasswordResets.SingleOrDefaultAsync(x => x.Token == token);
        _logger.LogDebug("Password reset entry: {Found}", entry is not null ? $"found for userId {entry.UserId}, expiresAt {entry.ExpiresAt}" : "not found");
        return entry;
    }

    public async Task AddAsync(PasswordResetEntry entry)
    {
        _logger.LogInformation("Adding password reset token entry for userId {UserId}, expiresAt {ExpiresAt}", entry.UserId, entry.ExpiresAt);
        await _dbContext.PasswordResets.AddAsync(entry);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Password reset token entry added for userId {UserId}", entry.UserId);
    }

    public async Task DeleteAsync(string token)
    {
        _logger.LogInformation("Deleting password reset token entry");
        var entity = await _dbContext.PasswordResets.SingleOrDefaultAsync(x => x.Token == token);
        if (entity is null)
        {
            _logger.LogDebug("Password reset token entry not found for deletion");
            return;
        }

        _dbContext.PasswordResets.Remove(entity);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Password reset token entry deleted for userId {UserId}", entity.UserId);
    }
}
