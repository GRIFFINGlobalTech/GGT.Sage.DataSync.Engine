//using System.Diagnostics;
//using Griffin.DataSync.Service.Interfaces;

//namespace Griffin.DataSync.Service.Services;

//public class SyncScheduler
//{
//    private readonly IEnumerable<ISyncJob> _jobs;
//    private readonly ILogger _logger;

//    public SyncScheduler(
//        IEnumerable<ISyncJob> jobs,
//        ILogger<SyncScheduler> logger)
//    {
//        _jobs = jobs;
//        _logger = logger;
//    }

//    public async Task RunAsync(
//        CancellationToken cancellationToken)
//    {
//        foreach (var job in _jobs)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            try
//            {
//                _logger.LogInformation(
//                    "Starting {Job}",
//                    job.JobName);

//                await job.ExecuteAsync(cancellationToken);

//                stopwatch.Stop();

//                _logger.LogInformation(
//                    "{Job} completed successfully in {ElapsedTime}.",
//                    job.JobName,
//                    FormatElapsedTime(stopwatch.Elapsed));
//            }
//            catch (Exception ex)
//            {
//                stopwatch.Stop();

//                _logger.LogError(
//                    ex,
//                    "{Job} failed after {ElapsedTime}.",
//                    job.JobName,
//                    FormatElapsedTime(stopwatch.Elapsed));
//            }
//        }
//    }

//    private static string FormatElapsedTime(TimeSpan elapsed)
//    {
//        if (elapsed.TotalSeconds < 1)
//        {
//            return $"{elapsed.TotalMilliseconds:N0} ms";
//        }

//        if (elapsed.TotalMinutes < 1)
//        {
//            return $"{elapsed.TotalSeconds:N2} seconds";
//        }

//        return $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
//    }
//}

using System.Diagnostics;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;

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
        var sageJobs =
            _jobs
                .Where(x =>
                    x.Pipeline ==
                    SyncPipeline.Sage)
                .ToList();

        var pickExceptionJobs =
            _jobs
                .Where(x =>
                    x.Pipeline ==
                    SyncPipeline.PickExceptions)
                .ToList();

        _logger.LogInformation(
            "Sage pipeline contains {Count} jobs.",
            sageJobs.Count);

        _logger.LogInformation(
            "Pick Exception pipeline contains {Count} jobs.",
            pickExceptionJobs.Count);

        var sagePipelineTask =
            RunPipelineAsync(
                "Sage",
                sageJobs,
                TimeSpan.FromSeconds(3),
                cancellationToken);

        var pickExceptionPipelineTask =
            RunPipelineAsync(
                "Pick Exceptions",
                pickExceptionJobs,
                TimeSpan.FromMinutes(1),
                cancellationToken);

        await Task.WhenAll(
            sagePipelineTask,
            pickExceptionPipelineTask);
    }

    private async Task RunPipelineAsync(
        string pipelineName,
        List<ISyncJob> jobs,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var cycleStopwatch =
                Stopwatch.StartNew();

            try
            {
                _logger.LogInformation(
                    "Starting {Pipeline} pipeline.",
                    pipelineName);

                foreach (var job in jobs)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await ExecuteJobAsync(
                        job,
                        cancellationToken);
                }

                cycleStopwatch.Stop();

                _logger.LogInformation(
                    "{Pipeline} pipeline completed in {ElapsedTime}.",
                    pipelineName,
                    FormatElapsedTime(
                        cycleStopwatch.Elapsed));
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                cycleStopwatch.Stop();

                _logger.LogInformation(
                    "{Pipeline} pipeline was cancelled after {ElapsedTime}.",
                    pipelineName,
                    FormatElapsedTime(
                        cycleStopwatch.Elapsed));

                break;
            }
            catch (Exception ex)
            {
                cycleStopwatch.Stop();

                _logger.LogError(
                    ex,
                    "{Pipeline} pipeline failed after {ElapsedTime}.",
                    pipelineName,
                    FormatElapsedTime(
                        cycleStopwatch.Elapsed));
            }

            try
            {
                _logger.LogInformation(
                    "Waiting {Interval} before next {Pipeline} pipeline cycle.",
                    interval,
                    pipelineName);

                await Task.Delay(
                    interval,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ExecuteJobAsync(
        ISyncJob job,
        CancellationToken cancellationToken)
    {
        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Starting {Job}.",
                job.JobName);

            await job.ExecuteAsync(
                cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "{Job} completed successfully in {ElapsedTime}.",
                job.JobName,
                FormatElapsedTime(
                    stopwatch.Elapsed));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "{Job} was cancelled after {ElapsedTime}.",
                job.JobName,
                FormatElapsedTime(
                    stopwatch.Elapsed));

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "{Job} failed after {ElapsedTime}.",
                job.JobName,
                FormatElapsedTime(
                    stopwatch.Elapsed));

            throw;
        }
    }

    private static string FormatElapsedTime(
        TimeSpan elapsed)
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