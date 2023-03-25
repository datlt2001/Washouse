using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class GetDeliveryPriceRequestModel
    {
        public int CenterId { get; set; }
        public ICollection<OrderDetailRequestModel> OrderDetails { get; set; }
        public string CustomerAddressString { get; set; }    
        public int CustomerWardId { get; set; }    
    }
}
