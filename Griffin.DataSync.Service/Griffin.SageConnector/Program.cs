using System.Data;
using System.Text.Json;
using Griffin.SageConnector.Infrastructure;
using Griffin.SageConnector.Repositories;
using Griffin.SageConnector.Services;

try
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine(
            "Command required.");

        Environment.Exit(1);
    }

    var command =
        args[0].ToLowerInvariant();
var ConnectionString = "Driver={MAS 90 4.0 ODBC Driver};UID=griffin;PWD=RPA4AAG;Company=AAG;Directory=\\\\md-sage\\Sage\\Sage 100 Advanced\\MAS90;Prefix=\\\\md-sage\\Sage\\Sage 100 Advanced\\MAS90\\SY\\, \\\\md-sage\\Sage\\Sage 100 Advanced\\MAS90\\==\\;ViewDLL=\\\\md-sage\\Sage\\Sage 100 Advanced\\MAS90\\HOME;LogFile=\\PVXODBC.LOG;RemotePVKIOHost=MD-SAGE;RemotePVKIOPort=20222;CacheSize=4;DirtyReads=1;BurstMode=1;StripTrailingSpaces=1;SERVER=NotTheServer;";
    var factory = new OdbcConnectionFactory(ConnectionString);

    var repository =
        new SageRepository(factory);

    switch (command)
    {
        // ========================================================
        // INVENTORY REPLENISHMENT
        // ========================================================

        case "inventory":
        {
            var inventory =
                await new InventoryReplenishmentService(
                    repository)
                    .ExecuteAsync();

            Console.WriteLine(
                JsonSerializer.Serialize(
                    inventory));

            break;
        }


        // ========================================================
        // TABLE SYNC
        // ========================================================

        default:
        {
            string tableName =
                command switch
                {
                    "ci_item" =>
                        "CI_ITEM",

                    "mb_binitem" =>
                        "MB_BinItem",

                    "mb_binlocation" =>
                        "MB_BinLocation",

                    "so_salesorderheader" =>
                        "SO_SalesOrderHeader",

                    "so_salesorderdetail" =>
                        "SO_SalesOrderDetail",

                    _ =>
                        throw new Exception(
                            $"Unknown command '{command}'.")
                };

            DataTable table =
                await repository.GetTableAsync(
                    tableName);

            var rows =
                table.Rows
                    .Cast<DataRow>()
                    .Select(r =>
                        table.Columns
                            .Cast<DataColumn>()
                            .ToDictionary(
                                c => c.ColumnName,
                                c =>
                                    r[c] == DBNull.Value
                                        ? null
                                        : r[c]));

            Console.WriteLine(
                JsonSerializer.Serialize(rows));

            break;
        }
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);

    Environment.Exit(-1);
}