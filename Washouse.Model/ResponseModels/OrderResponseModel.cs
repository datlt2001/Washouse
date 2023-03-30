using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.RequestModels;

namespace Washouse.Model.ResponseModels
{
    public class OrderResponseModel
    {
        public int CenterId { get; set; }
        //public virtual ICollection<OrderDetailResponseModel> CenterServices { get; set; }

        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public string Phone { get; set; }
        public string CenterAddress { get; set; }
        public double Distance { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool MonthOff { get; set; }
        public bool HasDelivery { get; set; }
        public virtual ICollection<CenterDeliveryPriceChartResponseModel> CenterDeliveryPrices { get; set; }
        public CenterLocationResponseModel CenterLocation { get; set; }
        public virtual ICollection<CenterOperatingHoursResponseModel> CenterOperatingHours { get; set; }
    }
}
