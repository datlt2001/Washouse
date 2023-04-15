using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderedDeliveryModel
    {
        public string DeliveryStatus { get; set; }
        public string AddressString { get; set; }
        public string WardName { get; set; }
        public string DistrictName { get; set; }
    }
}
