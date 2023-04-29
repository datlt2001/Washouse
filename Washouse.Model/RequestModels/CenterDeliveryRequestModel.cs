using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CenterDeliveryRequestModel
    {
        public bool HasDelivery { get; set; }
        public ICollection<CenterDeliveryPriceChartRequestModel> DeliveryPrice { get; set; }
    }
}
