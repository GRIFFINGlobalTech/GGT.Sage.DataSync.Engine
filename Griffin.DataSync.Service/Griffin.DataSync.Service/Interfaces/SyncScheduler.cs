using System.Diagnostics;
using Griffin.DataSync.Service.Interfaces;

namespace Griffin.DataSync.Service.Services;

public class SyncScheduler
{
    private readonly IEnumerable<ISyncJob> _jobs;
    private readonly ILogger _logger;

    public SyncScheduler(
        IEnumerable<ISyncJob> jobs,
        ILogger<SyncScheduler> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    public async Task RunAsync(
        CancellationToken cancellationToken)
    {
        foreach (var job in _jobs)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation(
                    "Starting {Job}",
                    job.JobName);

                await job.ExecuteAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    "{Job} completed successfully in {ElapsedTime}.",
                    job.JobName,
                    FormatElapsedTime(stopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "{Job} failed after {ElapsedTime}.",
                    job.JobName,
                    FormatElapsedTime(stopwatch.Elapsed));
            }
        }
    }

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 1)
        {
            return $"{elapsed.TotalMilliseconds:N0} ms";
        }

        if (elapsed.TotalMinutes < 1)
        {
            return $"{elapsed.TotalSeconds:N2} seconds";
        }

        return $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }
}