using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;
using Griffin.DataSync.Service.Repositories;
using Griffin.DataSync.Service.Services;

namespace Griffin.DataSync.Service.Jobs;

public class TableSyncJob : ISyncJob
{
    private readonly TableSyncDefinition _definition;

    private readonly SageConnectorRunner _connector;

    private readonly SqlRepo _sqlRepo;

    private readonly ILogger<TableSyncJob> _logger;

    public TableSyncJob(
        TableSyncDefinition definition,
        SageConnectorRunner connector,
        SqlRepo sqlRepo,
        ILogger<TableSyncJob> logger)
    {
        _definition = definition;
        _connector = connector;
        _sqlRepo = sqlRepo;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting {Job}",
            _definition.Name);

        var table =
            await _connector.ExecuteDataTableAsync(
                _definition.ConnectorCommand);

        _logger.LogInformation(
            "{Rows} rows received.",
            table.Rows.Count);

        await _sqlRepo.BulkInsertAsync(
            table,
            _definition.StageTable,
            cancellationToken);

        await _sqlRepo.ExecuteProcedureAsync(
            _definition.MergeProcedure,
            cancellationToken);

        _logger.LogInformation(
            "{Job} completed.",
            _definition.Name);
    }
}