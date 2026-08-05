using Griffin.DataSync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Infrastructure.ConnectionFactories
{
    public class OdbcConnectionFactory: IOdbcConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public OdbcConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<OdbcConnection> CreateConnectionAsync()
        {
            var connection =
                new OdbcConnection(_configuration.GetConnectionString("Sage"));

            await connection.OpenAsync();

            return connection;
        }
    }
}
