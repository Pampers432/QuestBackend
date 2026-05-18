using Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuestsApi.Services;

public class RegistrationTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RegistrationTokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;

    public RegistrationTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RegistrationTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cleanupInterval = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RegistrationTokenCleanupService started. Interval: {Interval}", _cleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<RegistrationTokenRepository>();
                var deletedCount = await repository.DeleteExpiredAsync();

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Deleted {Count} expired registration tokens", deletedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during expired registration token cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("RegistrationTokenCleanupService stopped");
    }
}
