using Griffin.DataSync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Services
{
    public class SyncEngine: ISyncEngine
    {
        private readonly IEnumerable<ISyncJob> _jobs;
        private readonly ILogger<SyncEngine> _logger;

        public SyncEngine(IEnumerable<ISyncJob> jobs, ILogger<SyncEngine> logger)
        {
            _jobs = jobs;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            foreach (var job in _jobs)
            {
                _logger.LogInformation("Starting {Job}", job.JobName);

                try
                {
                    await job.ExecuteAsync(cancellationToken);

                    _logger.LogInformation("{Job} completed.", job.JobName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Job} failed.", job.JobName);
                }
            }
        }
    }
}
