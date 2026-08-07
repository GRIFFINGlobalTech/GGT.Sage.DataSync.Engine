using Griffin.DataSync.Service.Services;

namespace Griffin.DataSync.Service;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var scheduler = scope.ServiceProvider
                    .GetRequiredService<SyncScheduler>();

                await scheduler.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while running scheduler.");
            }

            // Wake up every 30 seconds.
            // SyncEngine decides which jobs should run.
            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken);
        }

        _logger.LogInformation("Worker stopped.");
    }
}