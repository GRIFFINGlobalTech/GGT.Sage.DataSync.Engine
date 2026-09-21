using System;
using System.Collections.Generic;
using System.Text;

namespace Griffin.DataSync.Service.Models
{
    public class EmailOptions
    {
        public string TenantId { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string SenderEmail { get; set; } = string.Empty;

        public string SenderName { get; set; } = "A&A Global Warehouse Alerts";

        public int TimeoutSeconds { get; set; } = 30;
    }
}
