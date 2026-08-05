using Griffin.DataSync.Service.Interfaces;

namespace Griffin.DataSync.Service.Services;

public class SyncScheduler
{
    private readonly IEnumerable<ISyncJob> _jobs;
    private readonly ILogger<SyncScheduler> _logger;

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
            try
            {
                _logger.LogInformation(
                    "Starting {Job}",
                    job.JobName);

                await job.ExecuteAsync(cancellationToken);

                _logger.LogInformation(
                    "{Job} completed.",
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