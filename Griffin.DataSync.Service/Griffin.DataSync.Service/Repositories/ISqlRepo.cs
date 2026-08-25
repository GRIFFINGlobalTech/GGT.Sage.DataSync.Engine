using System.Data.Common;
using System.Data;

namespace Griffin.DataSync.Service.Interfaces;

public interface ISqlRepo
{
    Task BulkInsertAsync(DbDataReader reader, string destinationTable, CancellationToken cancellationToken);
    Task BulkInsertAsync(DataTable table, string destinationTable, CancellationToken cancellationToken);
    Task ExecuteProcedureAsync(string procedure,CancellationToken cancellationToken);
    Task ClearTableAsync(string tableName, CancellationToken cancellationToken);
    Task LogJobExecutionAsync(string jobName, DateTime startTime, DateTime endTime,long durationMs, string status, string? errorMessage,CancellationToken cancellationToken);
}