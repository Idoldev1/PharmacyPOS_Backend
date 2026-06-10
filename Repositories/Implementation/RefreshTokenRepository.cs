using Microsoft.EntityFrameworkCore;
using POS.API.Repositories.Interfaces;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Repositories.Implementation;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(AppDbContext dbContext, ILogger<RefreshTokenRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogDebug("RefreshTokenRepository initialized");
    }

    public async Task<RefreshTokenEntry?> GetByTokenAsync(string token)
    {
        _logger.LogDebug("Retrieving refresh token entry");
        var entry = await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);
        _logger.LogDebug("Refresh token entry: {Found}", entry is not null ? $"found for userId {entry.UserId}" : "not found");
        return entry;
    }

    public async Task AddAsync(RefreshTokenEntry entry)
    {
        _logger.LogInformation("Adding refresh token entry for userId {UserId}, expiresAt {ExpiresAt}", entry.UserId, entry.ExpiresAt);
        await _dbContext.RefreshTokens.AddAsync(entry);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Refresh token entry added for userId {UserId}", entry.UserId);
    }

    public async Task DeleteAsync(string token)
    {
        _logger.LogInformation("Deleting refresh token entry");
        var entity = await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);
        if (entity is null)
        {
            _logger.LogDebug("Refresh token entry not found for deletion");
            return;
        }

        _dbContext.RefreshTokens.Remove(entity);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Refresh token entry deleted for userId {UserId}", entity.UserId);
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        _logger.LogInformation("Deleting all refresh token entries for userId {UserId}", userId);
        var entities = await _dbContext.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();
        if (!entities.Any())
        {
            _logger.LogDebug("No refresh token entries found for userId {UserId}", userId);
            return;
        }

        _dbContext.RefreshTokens.RemoveRange(entities);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted {Count} refresh token entries for userId {UserId}", entities.Count, userId);
    }
}
