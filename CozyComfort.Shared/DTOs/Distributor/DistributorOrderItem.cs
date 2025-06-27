using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyComfort.Shared.DTOs.Distributor
{
    public class DistributorOrderItem
    {
        public int DistributorProductId { get; set; }
        public int Quantity { get; set; }
        public decimal RequestedPrice { get; set; }
    }
}
