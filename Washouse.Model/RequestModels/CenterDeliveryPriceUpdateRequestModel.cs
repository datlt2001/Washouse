using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CenterDeliveryPriceUpdateRequestModel
    {
        public bool HasDelivery { get; set; }
        public List<CenterDeliveryPriceChartUpdateRequestModel> DeliveryPriceCharts { get; set; }
    }
}
