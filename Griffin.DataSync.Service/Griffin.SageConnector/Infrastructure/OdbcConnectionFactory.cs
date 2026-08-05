using System.Data.Odbc;
using Griffin.SageConnector.Models;

namespace Griffin.SageConnector.Infrastructure;

public class OdbcConnectionFactory
{
    private readonly string conn;

    public OdbcConnectionFactory(string conn)
    {
        this.conn = conn;
    }

    public async Task<OdbcConnection> CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException(
                "Connection string is missing.");
        }

        var connection = new OdbcConnection(conn);

        await connection.OpenAsync();

        return connection;
    }
}