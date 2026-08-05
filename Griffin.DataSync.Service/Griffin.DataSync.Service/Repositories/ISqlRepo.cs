using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Repositories
{
    public interface ISqlRepo
    {
        Task BulkInsertAsync(DbDataReader table,string destinationTable,  CancellationToken cancellationToken);
        Task BulkInsertAsync<T>(IEnumerable<T> data,string tableName, CancellationToken cancellationToken);
        Task ExecuteProcedureAsync(string procedure, CancellationToken cancellationToken);
         Task BulkInsertAsync(
        DataTable table,
        string tableName,
        CancellationToken cancellationToken);
    }
}
