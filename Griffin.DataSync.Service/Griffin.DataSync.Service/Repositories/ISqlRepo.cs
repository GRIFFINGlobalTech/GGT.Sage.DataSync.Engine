using Griffin.DataSync.Service.Models;
using System.Data;
using System.Data.Common;

namespace Griffin.DataSync.Service.Interfaces;

public interface ISqlRepo
{
    Task BulkInsertAsync(DbDataReader reader, string destinationTable, CancellationToken cancellationToken);
    Task BulkInsertAsync(DataTable table, string destinationTable, CancellationToken cancellationToken);
    Task ExecuteProcedureAsync(string procedure,CancellationToken cancellationToken);
    Task ClearTableAsync(string tableName, CancellationToken cancellationToken);
    Task LogJobExecutionAsync(string jobName, DateTime startTime, DateTime endTime,long durationMs, string status, string? errorMessage,CancellationToken cancellationToken);
    Task<List<EmailQueueItem>> GetPendingEmailsAsync(CancellationToken cancellationToken);
    Task MarkEmailSentAsync(long emailId,CancellationToken cancellationToken);
    Task MarkEmailFailedAsync(long emailId, string errorMessage, CancellationToken cancellationToken);
}