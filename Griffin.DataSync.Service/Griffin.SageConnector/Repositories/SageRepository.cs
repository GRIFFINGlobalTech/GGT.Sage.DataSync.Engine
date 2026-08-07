using System.Data;
using System.Data.Odbc;
using Griffin.SageConnector.Infrastructure;

namespace Griffin.SageConnector.Repositories;

public class SageRepository
{
    private readonly OdbcConnectionFactory _factory;

    public SageRepository(OdbcConnectionFactory factory)
    {
        _factory = factory;
    }

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
}