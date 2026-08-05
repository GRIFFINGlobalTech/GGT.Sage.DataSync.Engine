using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Interfaces
{
    public interface ISageRepository
    {
        Task<DbDataReader> ExecuteQueryAsync(string sql, CancellationToken cancellationToken);
    }
}
