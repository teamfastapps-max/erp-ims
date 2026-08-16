using IMS.Services.Interfaces;

namespace IMS.Web.HostedServices
{
    public class PermissionSyncStartupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionSyncStartupService> _logger;
        private const int MaxRetries = 5;
        private const int RetryDelayMs = 5000;

        public PermissionSyncStartupService(IServiceProvider serviceProvider, ILogger<PermissionSyncStartupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => SyncWithRetryAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task SyncWithRetryAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IPermissionSyncService>();

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("[STARTUP] Attempt {Attempt}/{Max} - syncing permissions...", attempt, MaxRetries);
                    await syncService.SyncPermissionsAsync();
                    _logger.LogInformation("[STARTUP] Permission sync successful.");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[STARTUP] Attempt {Attempt}/{Max} failed", attempt, MaxRetries);
                    if (attempt < MaxRetries)
                        await Task.Delay(RetryDelayMs, ct);
                }
            }

            _logger.LogError("[STARTUP] Failed to sync permissions after {Max} attempts.", MaxRetries);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
