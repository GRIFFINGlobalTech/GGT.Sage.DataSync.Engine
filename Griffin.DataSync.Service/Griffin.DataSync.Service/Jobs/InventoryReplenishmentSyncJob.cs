using Griffin.DataSync.Service.Helpers;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;
using Griffin.DataSync.Service.Repositories;
using Griffin.DataSync.Service.Services;

namespace Griffin.DataSync.Service.Jobs;

public class InventoryReplenishmentSyncJob : ISyncJob
{
    private readonly SageConnectorRunner _runner;
    private readonly ISqlRepo _sqlRepository;
    private readonly RetryService _retryService;
    private readonly ILogger<InventoryReplenishmentSyncJob> _logger;

    private const string StageTable = "dbo.Stage_InventoryReplenishment";
    private const string MergeProcedure = "dbo.usp_MergeInventoryReplenishment";

    public string JobName => "Inventory Replenishment";
        public TimeSpan Interval => TimeSpan.FromMinutes(5);


    public InventoryReplenishmentSyncJob(
        SageConnectorRunner runner,
        ISqlRepo sqlRepository,
        RetryService retryService,
        ILogger<InventoryReplenishmentSyncJob> logger)
    {
        _runner = runner;
        _sqlRepository = sqlRepository;
        _retryService = retryService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {Job}", JobName);

        var inventory = await _retryService.ExecuteAsync(() =>
            _runner.ExecuteAsync<InventoryItem>("inventory"));

            _logger.LogInformation("Received {Count} inventory records.", inventory.Count);

foreach (var item in inventory.Take(5))
{
    _logger.LogInformation(
        "Item: {ItemCode}, Qty: {Qty}, QtyOnHand: {QtyOnHand}, Desc: {ItemDesc}",
        item.ItemCode,
        item.Qty,
        item.QtyOnHand,
        item.ItemDesc);
}

        await _retryService.ExecuteAsync(() =>
            _sqlRepository.BulkInsertAsync(
                inventory,
                StageTable,
                cancellationToken));

        await _retryService.ExecuteAsync(() =>
            _sqlRepository.ExecuteProcedureAsync(
                MergeProcedure,
                cancellationToken));

        _logger.LogInformation("{Job} completed successfully.", JobName);
    }
}