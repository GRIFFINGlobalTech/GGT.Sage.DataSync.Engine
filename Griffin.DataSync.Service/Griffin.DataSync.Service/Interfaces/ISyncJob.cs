using Griffin.DataSync.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Interfaces
{
    public interface ISyncJob
    {
        string JobName { get; }

        SyncPipeline Pipeline { get; }

        Task ExecuteAsync(
            CancellationToken cancellationToken);
    }
}
