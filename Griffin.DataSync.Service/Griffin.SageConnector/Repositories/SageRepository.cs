
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

    public async Task<List<InventoryItem>> ExecuteQueryAsync(
    string sql,
    DateTime fromDate,
    DateTime toDate)
{
    var result = new List<InventoryItem>();

    await using var connection =
        await _factory.CreateAsync();

    using var command = new OdbcCommand(sql, connection);


    // ODBC uses parameter order, not parameter names
    command.Parameters.Add(
        new OdbcParameter
        {
            OdbcType = OdbcType.DateTime,
            Value = fromDate
        });


    command.Parameters.Add(
        new OdbcParameter
        {
            OdbcType = OdbcType.DateTime,
            Value = toDate
        });

     command.CommandTimeout = 300; // 5 minutes
    using var reader =
        await command.ExecuteReaderAsync();


    // while (await reader.ReadAsync())
    // {
    //     result.Add(new InventoryItem
    //     {
    //         ItemCode = reader["ItemCode"]?.ToString() ?? "",

    //         Qty = Convert.ToDecimal(
    //             reader["Qty"]),

    //         QtyOnHand = Convert.ToDecimal(
    //             reader["QtyOnHand"]),

    //         ItemDesc = reader["ItemDesc"]?.ToString() ?? ""
    //     });
    // }
while (await reader.ReadAsync())
{
    for (int i = 0; i < reader.FieldCount; i++)
    {
        Console.Write($"{reader.GetName(i)} = {reader.GetValue(i)} | ");
    }

    Console.WriteLine();
}
    return result;
}
public async Task<List<InventoryBin>> GetInventoryAsync()
{
    const string sql = @"
SELECT
    MB.ItemCode,
    MB.QuantityOnHand,
    CI.ItemCodeDesc
FROM
    MB_BinItem MB,
    MB_BinLocation BL,
    CI_Item CI
WHERE
    MB.BinLocation = BL.BinLocation
AND MB.ItemCode = CI.ItemCode
AND BL.Active='Y'
AND (
      (
        MB.BinLocation LIKE '%A%'
        AND MB.WarehouseCode='001'
      )
      OR
      (
        MB.BinLocation LIKE '%A%'
        AND MB.WarehouseCode='003'
      )
    )";

    var result = new List<InventoryBin>();

    await using var connection = await _factory.CreateAsync();

    using var command = new OdbcCommand(sql, connection);

    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        result.Add(new InventoryBin
        {
            ItemCode = reader["ItemCode"].ToString()!,
            QuantityOnHand = Convert.ToDecimal(reader["QuantityOnHand"]),
            ItemDesc = reader["ItemCodeDesc"].ToString()!
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

    await using var connection = await _factory.CreateAsync();

    using var command = new OdbcCommand(sql, connection);

    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        result.Add(new SalesOrderLine
        {
            SalesOrderNo = reader["SalesOrderNo"].ToString()!,
            ItemCode = reader["ItemCode"].ToString()!,
            QuantityOrdered = Convert.ToDecimal(reader["QuantityOrdered"])
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

    await using var connection = await _factory.CreateAsync();

    using var command = new OdbcCommand(sql, connection);

    using var reader = await command.ExecuteReaderAsync();

    var fromDate = DateTime.Today.AddDays(-1);
var toDate = DateTime.Today.AddDays(14);

while (await reader.ReadAsync())
{
    if (reader["UDF_SHIP_BY_DATE"] == DBNull.Value)
        continue;

    var shipByDate = Convert.ToDateTime(reader["UDF_SHIP_BY_DATE"]);

    var orderType = reader["OrderType"]?.ToString() ?? "";

    if (orderType.Equals("Q", StringComparison.OrdinalIgnoreCase))
        continue;

    if (shipByDate < fromDate || shipByDate >= toDate)
        continue;

    result.Add(new SalesOrderHeader
    {
        SalesOrderNo = reader["SalesOrderNo"]?.ToString() ?? "",
        OrderType = orderType,
        ShipByDate = shipByDate
    });
}

    return result;
}
public async Task<DataTable> GetMBBinLocationAsync()
{
    const string sql = @"
        SELECT *
        FROM MB_BinLocation";

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
public async Task<DataTable> GetCIItemAsync()
{
    const string sql = @"
        SELECT *
        FROM CI_ITEM";

    using var connection =
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
}