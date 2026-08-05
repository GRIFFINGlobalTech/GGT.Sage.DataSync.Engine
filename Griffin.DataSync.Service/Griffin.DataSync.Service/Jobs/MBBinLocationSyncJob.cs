using Griffin.DataSync.Service.Helpers;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Repositories;
using Griffin.DataSync.Service.Services;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Data;

namespace Griffin.DataSync.Service.Jobs;

public class MBBinLocationSyncJob : ISyncJob
{
    private readonly SageConnectorRunner _runner;
    private readonly ISqlRepo _repository;
    private readonly RetryService _retry;
    private readonly ILogger<MBBinLocationSyncJob> _logger;

    public string JobName => "MB Bin Location";
    TimeSpan ISyncJob.Interval => TimeSpan.FromMinutes(5);
    public MBBinLocationSyncJob(
        SageConnectorRunner runner,
        ISqlRepo repository,
        RetryService retry,
        ILogger<MBBinLocationSyncJob> logger)
    {
        _runner = runner;
        _repository = repository;
        _retry = retry;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting {Job}",
            JobName);

        DataTable table =
            await _retry.ExecuteAsync(() =>
                _runner.ExecuteDataTableAsync(
                    "mb_binlocation"));

        _logger.LogInformation(
            "Received {Rows} rows.",
            table.Rows.Count);

        await _retry.ExecuteAsync(() =>
            _repository.BulkInsertAsync(
                table,
                "dbo.Stage_MB_BinLocation",
                cancellationToken));

        await _retry.ExecuteAsync(() =>
            _repository.ExecuteProcedureAsync(
                "dbo.usp_MergeMBBinLocation",
                cancellationToken));

        _logger.LogInformation(
            "{Job} completed successfully.",
            JobName);
    }
}