using Griffin.DataSync.Service.Infrastructure.ConnectionFactories;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Griffin.DataSync.Service.Repositories;

public class SqlRepo : ISqlRepo
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SqlRepo(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ============================================================
    // BULK INSERT DATATABLE
    // ============================================================

    public async Task BulkInsertAsync(DataTable table, string destinationTable, CancellationToken cancellationToken)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        if (table.Rows.Count == 0)
        {
            return;
        }

        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        // --------------------------------------------------------
        // Get actual SQL destination columns
        // --------------------------------------------------------

        var targetColumns =
            await GetDestinationColumnsAsync(
                connection,
                destinationTable,
                cancellationToken);

        if (targetColumns.Count == 0)
        {
            throw new InvalidOperationException(
                $"No columns found for destination table '{destinationTable}'.");
        }

        // --------------------------------------------------------
        // Create SqlBulkCopy
        // --------------------------------------------------------

        using var bulkCopy =
            new SqlBulkCopy(connection)
            {
                DestinationTableName = destinationTable,
                BulkCopyTimeout = 0,
                BatchSize = 5000,
                EnableStreaming = true
            };

        // VERY IMPORTANT
        //
        // Make sure there are absolutely no existing mappings.
        //
        bulkCopy.ColumnMappings.Clear();

        var mappedCount = 0;

        // --------------------------------------------------------
        // Map DataTable columns to SQL columns
        // --------------------------------------------------------

        foreach (DataColumn sourceColumn in table.Columns)
        {
            var sourceName =
                sourceColumn.ColumnName.Trim();

            if (string.IsNullOrWhiteSpace(sourceName))
                continue;

            // Only map columns that actually exist in SQL.
            var targetColumn =
                targetColumns.FirstOrDefault(
                    x => x.Equals(
                        sourceName,
                        StringComparison.OrdinalIgnoreCase));

            if (targetColumn == null)
            {
                // This is useful for debugging.
                // Sage may contain columns that your SQL table
                // does not contain.
                Console.WriteLine(
                    $"Skipping source column '{sourceName}' " +
                    $"because it does not exist in '{destinationTable}'.");

                continue;
            }

            bulkCopy.ColumnMappings.Add(
                sourceName,
                targetColumn);

            mappedCount++;
        }

        if (mappedCount == 0)
        {
            throw new InvalidOperationException(
                $"No matching columns found between Sage data " +
                $"and destination table '{destinationTable}'.");
        }

        // --------------------------------------------------------
        // Diagnostic logging
        // --------------------------------------------------------

        Console.WriteLine(
            $"Bulk inserting {table.Rows.Count} rows into {destinationTable}.");
  

        // --------------------------------------------------------
        // Write data
        // --------------------------------------------------------

        await bulkCopy.WriteToServerAsync(
            table,
            cancellationToken);
    }

    // ============================================================
    // GET DESTINATION TABLE COLUMNS
    // ============================================================

    private static async Task<List<string>> GetDestinationColumnsAsync(SqlConnection connection, string destinationTable,CancellationToken cancellationToken)
    {
        var result = new List<string>();

        var sql = $"""
            SELECT TOP (0) *
            FROM {destinationTable};
            """;

        using var command =
            new SqlCommand(
                sql,
                connection)
            {
                CommandTimeout = 0
            };

        using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            result.Add(
                reader.GetName(i));
        }

        return result;
    }

    // ============================================================
    // BULK INSERT DBDATAREADER
    // ============================================================

    public async Task BulkInsertAsync(DbDataReader reader, string destinationTable, CancellationToken cancellationToken)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var bulkCopy =
            new SqlBulkCopy(connection)
            {
                DestinationTableName = destinationTable,
                BulkCopyTimeout = 0,
                BatchSize = 5000,
                EnableStreaming = true
            };

        bulkCopy.ColumnMappings.Clear();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName =
                reader.GetName(i);

            bulkCopy.ColumnMappings.Add(
                columnName,
                columnName);
        }

        await bulkCopy.WriteToServerAsync(
            reader,
            cancellationToken);
    }

    // ============================================================
    // EXECUTE STORED PROCEDURE
    // ============================================================

    public async Task ExecuteProcedureAsync(string procedure, CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var command =
            new SqlCommand(
                procedure,
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 0
            };

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    public async Task ClearTableAsync(string tableName, CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var command =
            new SqlCommand(
                $"TRUNCATE TABLE {tableName}",
                connection)
            {
                CommandTimeout = 0
            };

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }
    public async Task LogJobExecutionAsync(string jobName,DateTime startTime, DateTime endTime, long durationMs, string status, string? errorMessage,CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        const string sql = """
        INSERT INTO dbo.GriffinRPASyncJobExecutionLog
        (
            JobName,
            StartTime,
            EndTime,
            DurationMs,
            Status,
            ErrorMessage
        )
        VALUES
        (
            @JobName,
            @StartTime,
            @EndTime,
            @DurationMs,
            @Status,
            @ErrorMessage
        );
        """;

        using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        command.Parameters.AddWithValue(
            "@JobName",
            jobName);

        command.Parameters.AddWithValue(
            "@StartTime",
            startTime);

        command.Parameters.AddWithValue(
            "@EndTime",
            endTime);

        command.Parameters.AddWithValue(
            "@DurationMs",
            durationMs);

        command.Parameters.AddWithValue(
            "@Status",
            status);

        command.Parameters.AddWithValue(
            "@ErrorMessage",
            (object?)errorMessage ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    public async Task<List<EmailQueueItem>> GetPendingEmailsAsync(
    CancellationToken cancellationToken)
    {
        var emails = new List<EmailQueueItem>();

        await using var connection = await _connectionFactory.CreateConnectionAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            "dbo.usp_GetPendingEmails";

        command.CommandType =
            System.Data.CommandType.StoredProcedure;

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            emails.Add(
                new EmailQueueItem
                {
                    ID = reader.GetInt64(
                        reader.GetOrdinal("ID")),

                    EmailType = reader["EmailType"]
                        ?.ToString() ?? string.Empty,

                    ReferenceID =
                        reader["ReferenceID"] == DBNull.Value
                            ? null
                            : Convert.ToInt64(
                                reader["ReferenceID"]),

                    ReferenceNo =
                        reader["ReferenceNo"] == DBNull.Value
                            ? null
                            : reader["ReferenceNo"]
                                .ToString(),

                    Recipients =
                        reader["Recipients"]
                            ?.ToString() ?? string.Empty,

                    Subject =
                        reader["Subject"]
                            ?.ToString() ?? string.Empty,

                    Body =
                        reader["Body"]
                            ?.ToString() ?? string.Empty,

                    Status =
                        reader["Status"]
                            ?.ToString() ?? string.Empty,

                    Attempts =
                        reader["Attempts"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                reader["Attempts"])
                });
        }

        return emails;
    }


    public async Task MarkEmailSentAsync(
        long emailId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();


        await using var command = connection.CreateCommand();

        command.CommandText =
            "dbo.usp_MarkEmailSent";

        command.CommandType =
            System.Data.CommandType.StoredProcedure;

        var parameter =
            command.CreateParameter();

        parameter.ParameterName =
            "@EmailID";

        parameter.Value =
            emailId;

        command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }


    public async Task MarkEmailFailedAsync(
        long emailId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();


        await using var command = connection.CreateCommand();

        command.CommandText =
            "dbo.usp_MarkEmailFailed";

        command.CommandType =
            System.Data.CommandType.StoredProcedure;

        var emailParameter =
            command.CreateParameter();

        emailParameter.ParameterName =
            "@EmailID";

        emailParameter.Value =
            emailId;

        command.Parameters.Add(emailParameter);

        var errorParameter =
            command.CreateParameter();

        errorParameter.ParameterName =
            "@ErrorMessage";

        errorParameter.Value =
            string.IsNullOrWhiteSpace(errorMessage)
                ? DBNull.Value
                : errorMessage.Length > 2000
                    ? errorMessage[..2000]
                    : errorMessage;

        command.Parameters.Add(errorParameter);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }
}