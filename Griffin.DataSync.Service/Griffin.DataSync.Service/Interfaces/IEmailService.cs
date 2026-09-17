using System;
using System.Collections.Generic;
using System.Text;

namespace Griffin.DataSync.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string recipients, string subject, string body, CancellationToken cancellationToken = default);
    }
}
