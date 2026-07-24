using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class PendingSaleExpiryService : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingSaleExpiryService> _logger;

    public PendingSaleExpiryService(IServiceScopeFactory scopeFactory, ILogger<PendingSaleExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
                await saleService.ExpireDueSalesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while expiring pending sales");
            }

            try
            {
                await Task.Delay(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Shutdown requested during the delay — exit the loop.
            }
        }
    }
}
