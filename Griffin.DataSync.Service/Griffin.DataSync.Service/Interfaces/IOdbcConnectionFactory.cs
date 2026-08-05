using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Interfaces
{
    public interface IOdbcConnectionFactory
    {
        Task<OdbcConnection> CreateConnectionAsync();

    }
}
