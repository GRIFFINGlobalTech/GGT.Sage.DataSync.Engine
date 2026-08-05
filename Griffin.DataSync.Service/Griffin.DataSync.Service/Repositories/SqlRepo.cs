using Griffin.DataSync.Service.Interfaces;
using Microsoft.AspNetCore.Connections;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Repositories
{
    public class SqlRepo: ISqlRepo
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public SqlRepo(ISqlConnectionFactory factory)
        {
            _connectionFactory = factory;
        }
public async Task BulkInsertAsync<T>(
    IEnumerable<T> data,
    string tableName,
    CancellationToken cancellationToken)
{
    var table = ToDataTable(data);

    await using var connection =  await _connectionFactory.CreateConnectionAsync();

    using var bulk =
        new SqlBulkCopy(connection);

    bulk.DestinationTableName = tableName;

    foreach (DataColumn column in table.Columns)
    {
        bulk.ColumnMappings.Add(
            column.ColumnName,
            column.ColumnName);
    }

    await bulk.WriteToServerAsync(
        table,
        cancellationToken);
}
        public async Task BulkInsertAsync(DbDataReader reader,string destinationTable, CancellationToken cancellationToken)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            using var bulkCopy =  new SqlBulkCopy(connection);

            bulkCopy.DestinationTableName = destinationTable;

            bulkCopy.BatchSize = 5000;

            bulkCopy.BulkCopyTimeout = 0;

            await bulkCopy.WriteToServerAsync(
                reader,
                cancellationToken);
        }
public async Task BulkInsertAsync(
    DataTable table,
    string tableName,
    CancellationToken cancellationToken)
{
    await using var connection =
        await _connectionFactory.CreateConnectionAsync();

    using var bulk = new SqlBulkCopy(connection)
    {
        DestinationTableName = tableName,
        BulkCopyTimeout = 0
    };

    foreach (DataColumn column in table.Columns)
    {
        bulk.ColumnMappings.Add(
            column.ColumnName,
            column.ColumnName);
    }

    await bulk.WriteToServerAsync(
        table,
        cancellationToken);
}
        public async Task ExecuteProcedureAsync( string procedure,  CancellationToken cancellationToken)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync();

            await using var command =  new SqlCommand(procedure, connection);

            command.CommandType =  CommandType.StoredProcedure;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        private static DataTable ToDataTable<T>(
    IEnumerable<T> data)
{
    var table = new DataTable();

    var properties =
        typeof(T).GetProperties();

    foreach (var property in properties)
    {
        table.Columns.Add(
            property.Name,
            Nullable.GetUnderlyingType(property.PropertyType)
            ?? property.PropertyType);
    }

    foreach (var item in data)
    {
        var row = table.NewRow();

        foreach (var property in properties)
        {
            row[property.Name] =
                property.GetValue(item)
                ?? DBNull.Value;
        }

        table.Rows.Add(row);
    }

    return table;
}
    }
}
