using Griffin.DataSync.Service.Configuration;
using Griffin.DataSync.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace Griffin.DataSync.Service.Services;

public class SyncEngine : ISyncEngine
{
    private readonly IEnumerable<ISyncJob> _jobs;
    private readonly ILogger<SyncEngine> _logger;
    private readonly JobScheduleOptions _options;

    // Stores the last execution time of every job
    private readonly Dictionary<string, DateTime> _lastRun = new();

    public SyncEngine(
        IEnumerable<ISyncJob> jobs,
        ILogger<SyncEngine> logger,
        IOptions<JobScheduleOptions> options)
    {
        _jobs = jobs;
        _logger = logger;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        foreach (var job in _jobs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_lastRun.ContainsKey(job.JobName))
            {
                _lastRun[job.JobName] = DateTime.MinValue;
            }

            // Read interval from appsettings.json
            if (!_options.JobSchedules.TryGetValue(
                    job.JobName,
                    out var intervalMinutes))
            {
                intervalMinutes = 5; // Default if not configured
            }

            // Skip if it's not time yet
            if (DateTime.Now - _lastRun[job.JobName]
                < TimeSpan.FromMinutes(intervalMinutes))
            {
                continue;
            }

            _logger.LogInformation(
                "Starting {Job}",
                job.JobName);

            try
            {
                await job.ExecuteAsync(cancellationToken);

                _lastRun[job.JobName] = DateTime.Now;

                _logger.LogInformation(
                    "{Job} completed successfully.",
                    job.JobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Job} failed.",
                    job.JobName);
            }
        }
    }
}