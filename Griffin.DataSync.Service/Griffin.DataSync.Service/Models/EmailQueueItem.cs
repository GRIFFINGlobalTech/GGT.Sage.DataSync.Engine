using System;
using System.Collections.Generic;
using System.Text;

namespace Griffin.DataSync.Service.Models
{
    public class EmailQueueItem
    {
        public long ID { get; set; }

        public string EmailType { get; set; } = string.Empty;

        public long? ReferenceID { get; set; }

        public string? ReferenceNo { get; set; }

        public string Recipients { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int Attempts { get; set; }
    }
}
