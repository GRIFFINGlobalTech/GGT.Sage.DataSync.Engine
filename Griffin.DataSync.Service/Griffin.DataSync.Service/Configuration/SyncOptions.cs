using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Configuration
{
    public class SyncOptions
    {
        public int IntervalSeconds { get; set; }
        public int RetryCount { get; set; }
        public int BatchSize { get; set; }
    }
}
