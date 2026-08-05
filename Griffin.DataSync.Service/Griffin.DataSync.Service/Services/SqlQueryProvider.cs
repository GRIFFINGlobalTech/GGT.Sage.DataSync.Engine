using Griffin.DataSync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Services
{
    public class SqlQueryProvider : ISqlQueryProvider
    {
        private readonly Dictionary<string, string> _cache = new();

        public async Task<string> GetAsync(string name)
        {
            if (_cache.TryGetValue(name, out var sql))
                return sql;

            var file = Path.Combine(
                AppContext.BaseDirectory,
                "Queries",
                $"{name}.sql");

            sql = await File.ReadAllTextAsync(file);

            _cache[name] = sql;

            return sql;
        }
    }
}
