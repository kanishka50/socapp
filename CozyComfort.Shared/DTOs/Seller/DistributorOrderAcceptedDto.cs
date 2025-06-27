using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyComfort.Shared.DTOs.Seller
{
    public class DistributorOrderAcceptedDto
    {
        public string DistributorOrderNumber { get; set; } = string.Empty;
        public string SellerOrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
