using System.Data;
using System.Text.Json;
using Griffin.SageConnector.Infrastructure;
using Griffin.SageConnector.Repositories;
using Griffin.SageConnector.Services;

try
{
    if (args.Length == 0)
        throw new Exception("Command required.");

    var command = args[0].ToLower();

    var factory =
        new OdbcConnectionFactory(
            "DSN=SOTAMAS90;UID=griffin;PWD=RPA4AAG;");

    var repository = new SageRepository(factory);

    switch (command)
    {
        case "inventory":

            var inventory =
                await new InventoryReplenishmentService(repository)
                    .ExecuteAsync();

            Console.WriteLine(
                JsonSerializer.Serialize(inventory));

            break;

        default:

            string tableName = command switch
            {
                "ci_item" => "CI_ITEM",

                "mb_binitem" => "MB_BinItem",

                "mb_binlocation" => "MB_BinLocation",

                "so_salesorderheader" => "SO_SalesOrderHeader",

                "so_salesorderdetail" => "SO_SalesOrderDetail",

                _ => throw new Exception($"Unknown command {command}")
            };

            DataTable table =
                await repository.GetTableAsync(tableName);

            var rows =
                table.Rows.Cast<DataRow>()
                .Select(r =>
                    table.Columns.Cast<DataColumn>()
                    .ToDictionary(
                        c => c.ColumnName,
                        c => r[c] == DBNull.Value
                            ? null
                            : r[c]));

            Console.WriteLine(
                JsonSerializer.Serialize(rows));

            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    Environment.Exit(-1);
}