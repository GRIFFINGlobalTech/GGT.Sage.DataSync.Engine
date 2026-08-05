using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Interfaces
{
    public interface ISqlQueryProvider
    {
        Task<string> GetAsync(string name);

    }
}
