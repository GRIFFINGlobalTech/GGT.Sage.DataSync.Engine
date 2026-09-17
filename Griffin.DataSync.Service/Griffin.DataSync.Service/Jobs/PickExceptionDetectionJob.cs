using Griffin.DataSync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Griffin.DataSync.Service.Jobs
{
    public class PickExceptionDetectionJob : ISyncJob
    {
        private readonly ISqlRepo _sqlRepo;
        private readonly ILogger<PickExceptionDetectionJob> _logger;

        public string JobName =>
            "Detect Pick Exceptions";

        public TimeSpan Interval =>
            TimeSpan.FromMinutes(1);

        public PickExceptionDetectionJob(
            ISqlRepo sqlRepo,
            ILogger<PickExceptionDetectionJob> logger)
        {
            _sqlRepo = sqlRepo;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            var stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation(
                "Starting {Job} at {StartTime}",
                JobName,
                startTime);

            try
            {
                await _sqlRepo.ExecuteProcedureAsync(
                    "dbo.usp_DetectPickExceptions",
                    cancellationToken);

                await _sqlRepo.ExecuteProcedureAsync(
                    "dbo.usp_QueuePickExceptionEmails",
                    cancellationToken);

                stopwatch.Stop();

                var endTime = DateTime.Now;

                var durationMs =
                    stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "{Job} completed at {EndTime}. Duration: {DurationMs} ms ({DurationSeconds:F2} seconds)",
                    JobName,
                    endTime,
                    durationMs,
                    stopwatch.Elapsed.TotalSeconds);

                await _sqlRepo.LogJobExecutionAsync(
                    JobName,
                    startTime,
                    endTime,
                    durationMs,
                    "SUCCESS",
                    null,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                var endTime = DateTime.Now;

                var durationMs =
                    stopwatch.ElapsedMilliseconds;

                _logger.LogError(
                    ex,
                    "{Job} failed after {DurationMs} ms",
                    JobName,
                    durationMs);

                try
                {
                    await _sqlRepo.LogJobExecutionAsync(
                        JobName,
                        startTime,
                        endTime,
                        durationMs,
                        "FAILED",
                        ex.Message,
                        cancellationToken);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(
                        logEx,
                        "Failed to write execution history for {Job}",
                        JobName);
                }

                throw;
            }
        }
    }
}
