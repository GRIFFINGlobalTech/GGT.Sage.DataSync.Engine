using Griffin.DataSync.Service.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Infrastructure.ConnectionFactories
{
    public class SqlConnectionFactory: ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            var connection =
                new SqlConnection(
                    _configuration.GetConnectionString("RPA"));

            await connection.OpenAsync();

            return connection;
        }
    }
}
