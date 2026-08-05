using Griffin.DataSync.Service.Interfaces;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Repositories
{
    internal class SageRepository: ISageRepository
    {
        private readonly IOdbcConnectionFactory _factory;

        public SageRepository(
            IOdbcConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<DbDataReader> ExecuteQueryAsync(string sql, CancellationToken cancellationToken)
        {
            var connection = await _factory.CreateConnectionAsync();

           
            var command = new OdbcCommand(sql, connection);

            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
        }
    }
}
