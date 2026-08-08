using System.Data;
using System.Data.Odbc;
using Griffin.SageConnector.Infrastructure;
using Griffin.SageConnector.Models;

namespace Griffin.SageConnector.Repositories;

public class SageRepository
{
    private readonly OdbcConnectionFactory _factory;

    public SageRepository(OdbcConnectionFactory factory)
    {
        _factory = factory;
    }

    // ============================================================
    // GENERIC TABLE SYNC
    // Used by:
    // ci_item
    // mb_binitem
    // mb_binlocation
    // so_salesorderheader
    // so_salesorderdetail
    // ============================================================

    public async Task<DataTable> GetTableAsync(string tableName)
    {
        var sql = $"SELECT * FROM {tableName}";

        await using var connection =
            await _factory.CreateAsync();

        using var command =
            new OdbcCommand(sql, connection);

        command.CommandTimeout = 0;

        using var reader =
            await command.ExecuteReaderAsync();

        var table = new DataTable();

        table.Load(reader);

        return table;
    }


    // ============================================================
    // INVENTORY REPLENISHMENT
    // ============================================================

    public async Task<List<InventoryBin>> GetInventoryAsync()
    {
        const string sql = @"
SELECT
    MB.ItemCode,
    MB.QuantityOnHand,
    CI.ItemCodeDesc
FROM MB_BinItem MB
INNER JOIN MB_BinLocation BL
    ON MB.BinLocation = BL.BinLocation
INNER JOIN CI_Item CI
    ON MB.ItemCode = CI.ItemCode
WHERE
    BL.Active = 'Y'
    AND
    (
        (
            MB.BinLocation LIKE '%A%'
            AND MB.WarehouseCode = '001'
        )
        OR
        (
            MB.BinLocation LIKE '%A%'
            AND MB.WarehouseCode = '003'
        )
    )";

        var result = new List<InventoryBin>();

        await using var connection =
            await _factory.CreateAsync();

        using var command =
            new OdbcCommand(sql, connection);

        command.CommandTimeout = 300;

        using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new InventoryBin
            {
                ItemCode =
                    reader["ItemCode"]?.ToString() ?? "",

                QuantityOnHand =
                    reader["QuantityOnHand"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            reader["QuantityOnHand"]),

                ItemDesc =
                    reader["ItemCodeDesc"]?.ToString() ?? ""
            });
        }

        return result;
    }


    public async Task<List<SalesOrderLine>> GetSalesOrderLinesAsync()
    {
        const string sql = @"
SELECT
    SalesOrderNo,
    ItemCode,
    QuantityOrdered
FROM SO_SalesOrderDetail";

        var result = new List<SalesOrderLine>();

        await using var connection =
            await _factory.CreateAsync();

        using var command =
            new OdbcCommand(sql, connection);

        command.CommandTimeout = 300;

        using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new SalesOrderLine
            {
                SalesOrderNo =
                    reader["SalesOrderNo"]?.ToString() ?? "",

                ItemCode =
                    reader["ItemCode"]?.ToString() ?? "",

                QuantityOrdered =
                    reader["QuantityOrdered"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            reader["QuantityOrdered"])
            });
        }

        return result;
    }


    public async Task<List<SalesOrderHeader>> GetSalesOrderHeadersAsync()
    {
        const string sql = @"
SELECT
    SalesOrderNo,
    OrderType,
    UDF_SHIP_BY_DATE
FROM SO_SalesOrderHeader";

        var result = new List<SalesOrderHeader>();

        await using var connection =
            await _factory.CreateAsync();

        using var command =
            new OdbcCommand(sql, connection);

        command.CommandTimeout = 300;

        using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (reader["UDF_SHIP_BY_DATE"] == DBNull.Value)
                continue;

            var shipByDate =
                Convert.ToDateTime(
                    reader["UDF_SHIP_BY_DATE"]);

            var orderType =
                reader["OrderType"]?.ToString() ?? "";

            result.Add(new SalesOrderHeader
            {
                SalesOrderNo =
                    reader["SalesOrderNo"]?.ToString() ?? "",

                OrderType = orderType,

                ShipByDate = shipByDate
            });
        }

        return result;
    }
}