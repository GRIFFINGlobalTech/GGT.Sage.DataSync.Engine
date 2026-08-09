using System.Data;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Services;

namespace Griffin.DataSync.Service.Jobs;

public class InventoryReplenishmentSyncJob : ISyncJob
{
    private readonly SageConnectorRunner _connector;
    private readonly ISqlRepo _sqlRepo;
    private readonly ILogger<InventoryReplenishmentSyncJob> _logger;

    public string JobName => "Inventory Replenishment";

    public TimeSpan Interval =>
        TimeSpan.FromMinutes(5);

    public InventoryReplenishmentSyncJob(
        SageConnectorRunner connector,
        ISqlRepo sqlRepo,
        ILogger<InventoryReplenishmentSyncJob> logger)
    {
        _connector = connector;
        _sqlRepo = sqlRepo;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting {Job}",
            JobName);

        var inventory =
            await _connector.ExecuteAsync<InventoryItem>(
                "inventory");

        _logger.LogInformation(
            "Received {Count} inventory records.",
            inventory.Count);

        if (inventory.Count == 0)
        {
            _logger.LogInformation(
                "No inventory records to process.");

            return;
        }

        var table =
            new DataTable();

        table.Columns.Add(
            "ItemCode",
            typeof(string));

        table.Columns.Add(
            "Qty",
            typeof(decimal));

        table.Columns.Add(
            "QtyOnHand",
            typeof(decimal));

        table.Columns.Add(
            "ItemDesc",
            typeof(string));

        foreach (var item in inventory)
        {
            table.Rows.Add(
                item.ItemCode,
                item.Qty,
                item.QtyOnHand,
                item.ItemDesc);
        }

        await _sqlRepo.BulkInsertAsync(
            table,
            "dbo.Stage_InventoryReplenishment",
            cancellationToken);

        _logger.LogInformation(
            "Inventory stage insert completed.");

        await _sqlRepo.ExecuteProcedureAsync(
            "dbo.usp_MergeInventoryReplenishment",
            cancellationToken);

        _logger.LogInformation(
            "{Job} completed successfully.",
            JobName);
    }
}