using System.Data;
using System.Text.Json;
using Griffin.SageConnector.Infrastructure;
using Griffin.SageConnector.Repositories;
using Griffin.SageConnector.Services;

try
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("Command is required.");
        Environment.Exit(1);
    }

    var command = args[0].ToLower();

    var factory =
        new OdbcConnectionFactory(
            "DSN=SOTAMAS90;UID=griffin;PWD=RPA4AAG;");

    var repository = new SageRepository(factory);

    switch (command)
    {
        case "inventory":
        {
            var inventory =
                await new InventoryReplenishmentService(repository)
                    .ExecuteAsync();

            Console.WriteLine(
                JsonSerializer.Serialize(inventory));

            break;
        }

        case "ci_item":
        {
            DataTable table =
                await repository.GetCIItemAsync();

            var rows = table.Rows.Cast<DataRow>()
                .Select(r => table.Columns.Cast<DataColumn>()
                    .ToDictionary(
                        c => c.ColumnName,
                        c => r[c] == DBNull.Value ? null : r[c]));

            Console.WriteLine(
                JsonSerializer.Serialize(rows));

            break;
        }
        case "mb_binlocation":
{
    DataTable table =
        await repository.GetMBBinLocationAsync();

    var rows = table.Rows.Cast<DataRow>()
        .Select(r => table.Columns.Cast<DataColumn>()
            .ToDictionary(
                c => c.ColumnName,
                c => r[c] == DBNull.Value
                        ? null
                        : r[c]));

    Console.WriteLine(
        JsonSerializer.Serialize(rows));

    break;
}

        default:
            throw new Exception($"Unknown command '{command}'.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    Environment.Exit(-1);
}