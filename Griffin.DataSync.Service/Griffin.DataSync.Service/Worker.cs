using Griffin.DataSync.Service.Services;
using Microsoft.Extensions.DependencyInjection;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope =
                _scopeFactory.CreateScope();

            var scheduler =
                scope.ServiceProvider
                     .GetRequiredService<SyncScheduler>();

            await scheduler.RunAsync(stoppingToken);

            _logger.LogInformation(
                "Waiting 5 minutes...");

            await Task.Delay(
                TimeSpan.FromMinutes(5),
                stoppingToken);
        }
    }
}