using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Models
{
    public class InventoryAvailability
    {
        public string ItemCode { get; set; } = string.Empty;

        public decimal Qty { get; set; }

        public decimal QtyOnHand { get; set; }

        public string ItemDescription { get; set; } = string.Empty;
    }
}
