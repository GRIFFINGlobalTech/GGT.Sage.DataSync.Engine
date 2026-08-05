using Griffin.DataSync.Service.Helpers;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Repositories;
using Griffin.DataSync.Service.Services;

namespace Griffin.DataSync.Service.Jobs;

public class CIItemSyncJob : ISyncJob
{
    private readonly SageConnectorRunner _runner;
    private readonly ISqlRepo _sqlRepo;
    private readonly ILogger<CIItemSyncJob> _logger;
    private readonly RetryService _retryService;

    private const string StageTable = "dbo.Stage_CI_ITEM";
    private const string MergeProcedure = "dbo.usp_MergeCIItem";

    public string JobName => "CI Item Sync";
    public TimeSpan Interval => TimeSpan.FromMinutes(5);


    public CIItemSyncJob(
        SageConnectorRunner runner,
        ISqlRepo sqlRepo,
        RetryService retryService,
        ILogger<CIItemSyncJob> logger)
    {
        _runner = runner;
        _sqlRepo = sqlRepo;
        _retryService = retryService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {Job}", JobName);

        // Step 1: Execute the connector
        var table = await _retryService.ExecuteAsync(() =>
            _runner.ExecuteDataTableAsync("ci_item"));

        _logger.LogInformation(
            "Received {Rows} rows from Sage.",
            table.Rows.Count);

        if (table.Rows.Count == 0)
        {
            _logger.LogInformation(
                "No records returned from Sage.");

            return;
        }

        // Step 2: Bulk insert into staging
        await _retryService.ExecuteAsync(() =>
            _sqlRepo.BulkInsertAsync(
                table,
                StageTable,
                cancellationToken));

        _logger.LogInformation(
            "Bulk insert completed.");

        // Step 3: Merge staging into final table
        await _retryService.ExecuteAsync(() =>
            _sqlRepo.ExecuteProcedureAsync(
                MergeProcedure,
                cancellationToken));

        _logger.LogInformation(
            "{Job} completed successfully.",
            JobName);
    }
}