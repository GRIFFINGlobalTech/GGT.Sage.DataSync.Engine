using Griffin.DataSync.Service.Configuration;
using Griffin.DataSync.Service.Interfaces;

namespace Griffin.DataSync.Service.Services;

public class SyncEngine : ISyncEngine
{
    private readonly IEnumerable<ISyncJob> _jobs;
    private readonly ILogger<SyncEngine> _logger;

    private readonly List<JobSchedule> _schedules =
    [
        new()
        {
            JobName = "Inventory Replenishment",
            Interval = TimeSpan.FromMinutes(5)
        },

        new()
        {
            JobName = "CI Item",
            Interval = TimeSpan.FromHours(12)
        },

        new()
        {
            JobName = "MB Bin Location",
            Interval = TimeSpan.FromHours(12)
        },

        new()
        {
            JobName = "MB Bin Item",
            Interval = TimeSpan.FromMinutes(10)
        },

        new()
        {
            JobName = "SO Sales Order Header",
            Interval = TimeSpan.FromMinutes(2)
        },

        new()
        {
            JobName = "SO Sales Order Detail",
            Interval = TimeSpan.FromMinutes(2)
        }
    ];

    public SyncEngine(
        IEnumerable<ISyncJob> jobs,
        ILogger<SyncEngine> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        foreach (var job in _jobs)
        {
            var schedule =
                _schedules.First(x => x.JobName == job.JobName);

            if (DateTime.Now - schedule.LastRun < schedule.Interval)
                continue;

            _logger.LogInformation(
                "Starting {Job}",
                job.JobName);

            try
            {
                await job.ExecuteAsync(cancellationToken);

                schedule.LastRun = DateTime.Now;

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