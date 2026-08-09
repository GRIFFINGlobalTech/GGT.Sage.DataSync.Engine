using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;
using Griffin.DataSync.Service.Services;
using System.Data;

public class TableSyncJob : ISyncJob
{
    private readonly SageConnectorRunner _connector;
    private readonly ISqlRepo _sqlRepo;
    private readonly TableSyncDefinition _definition;
    private readonly ILogger _logger;

    public string JobName =>
        _definition.JobName;

    public TimeSpan Interval =>
        TimeSpan.FromMinutes(
            _definition.IntervalMinutes);

    public TableSyncJob(
        SageConnectorRunner connector,
        ISqlRepo sqlRepo,
        TableSyncDefinition definition,
        ILogger<TableSyncJob> logger)
    {
        _connector = connector;
        _sqlRepo = sqlRepo;
        _definition = definition;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting {Job}",
            JobName);

        // ========================================================
        // GET DATA FROM SAGE
        // ========================================================

        var table =
            await _connector.ExecuteDataTableAsync(
                _definition.Command);

        _logger.LogInformation(
            "Received {Count} rows from Sage for {Job}.",
            table.Rows.Count,
            JobName);

        if (table.Rows.Count == 0)
        {
            _logger.LogInformation(
                "No rows returned for {Job}.",
                JobName);

            return;
        }
       
        // ========================================================
        // BULK INSERT
        // ========================================================

        await _sqlRepo.BulkInsertAsync(
            table,
            _definition.StageTable,
            cancellationToken);

        _logger.LogInformation(
            "Bulk insert completed for {Job}.",
            JobName);

        // ========================================================
        // MERGE
        // ========================================================

        await _sqlRepo.ExecuteProcedureAsync(
            _definition.MergeProcedure,
            cancellationToken);

        _logger.LogInformation(
            "{Job} completed successfully.",
            JobName);
    }
}