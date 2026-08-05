using Griffin.DataSync.Service.Configuration;
using Griffin.DataSync.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace Griffin.DataSync.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISyncEngine _syncEngine;
        private readonly SyncOptions _syncOptions;

        public Worker(ILogger<Worker> logger, ISyncEngine syncEngine, IOptions<SyncOptions> options)
        {
            _logger = logger;
            _syncEngine = syncEngine;
            _syncOptions = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await _syncEngine.RunAsync(stoppingToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(_syncOptions.IntervalSeconds),
                    stoppingToken);
            }
        }
    }
}
