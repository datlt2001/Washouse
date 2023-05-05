using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CenterDeliveryPriceChartUpdateRequestModel
    {
        public int Id { get; set; }
        public decimal? MaxDistance { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
    }
}
