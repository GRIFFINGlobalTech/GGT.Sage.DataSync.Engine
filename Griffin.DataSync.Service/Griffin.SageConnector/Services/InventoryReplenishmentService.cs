using Griffin.SageConnector.Models;
using Griffin.SageConnector.Repositories;

namespace Griffin.SageConnector.Services;

public class InventoryReplenishmentService
{
    private readonly SageRepository _repo;

    public InventoryReplenishmentService(SageRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<InventoryItem>> ExecuteAsync()
    {
        var inventory =
            await _repo.GetInventoryAsync();

        var lines =
            await _repo.GetSalesOrderLinesAsync();

        var headers =
            await _repo.GetSalesOrderHeadersAsync();

        var fromDate = DateTime.Today.AddDays(-1);

        var toDate = DateTime.Today.AddDays(14);

        var validOrders =
            headers
                .Where(x =>
                    x.OrderType != "Q" &&
                    x.ShipByDate >= fromDate &&
                    x.ShipByDate < toDate)
                .Select(x => x.SalesOrderNo)
                .ToHashSet();

        var demand =
            lines
                .Where(x => validOrders.Contains(x.SalesOrderNo))
                .GroupBy(x => x.ItemCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.QuantityOrdered));

        var result =
            inventory
                .GroupBy(x => new
                {
                    x.ItemCode,
                    x.ItemDesc
                })
                .Select(g =>
                {
                    demand.TryGetValue(
                        g.Key.ItemCode,
                        out var ordered);

                    var onHand =
                        g.Sum(x => x.QuantityOnHand);

                    return new InventoryItem
                    {
                        ItemCode = g.Key.ItemCode,
                        ItemDesc = g.Key.ItemDesc,
                        QtyOnHand = onHand,
                        Qty = onHand - ordered
                    };
                })
                .Where(x =>
                    x.Qty < 0 &&
                    x.QtyOnHand > 0)
                .OrderBy(x => x.ItemCode)
                .ToList();

        return result;
    }
}