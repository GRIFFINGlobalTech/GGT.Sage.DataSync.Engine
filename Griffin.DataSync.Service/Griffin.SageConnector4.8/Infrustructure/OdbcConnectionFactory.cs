using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.SageConnector4._8.Infrustructure
{
    public class OdbcConnectionFactory
    {
        private readonly string _connectionString;

        public OdbcConnectionFactory()
        {
            _connectionString = ConfigurationManager.AppSettings["ConnectionString"];

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new Exception(
                    "Connection string was not found in App.config.");
            }
        }

        public OdbcConnection CreateConnection()
        {
            return new OdbcConnection(_connectionString);
        }
    }
}
