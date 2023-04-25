using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.ResponseModels
{
    public class CenterResponseModel
    {
        public CenterResponseModel()
        {
            CenterServices = new HashSet<CenterServiceResponseModel>();
            CenterOperatingHours = new HashSet<CenterOperatingHoursResponseModel>();
            CenterDeliveryPrices = new HashSet<CenterDeliveryPriceChartResponseModel>();
        }

        public int Id { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public virtual ICollection<CenterServiceResponseModel> CenterServices { get; set; }

        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public string Phone { get; set; }
        public string CenterAddress { get; set; }
        public double Distance { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool MonthOff { get; set; }
        public bool HasDelivery { get; set; }
        public bool HasOnlinePayment { get; set; }
        public bool IsOpening { get; set; }
        public int[] Ratings { get; set; }
        public DateTime? LastDeactivate { get; set; }
        public int NumOfPromotionAvailable { get; set; }
        public virtual ICollection<CenterDeliveryPriceChartResponseModel> CenterDeliveryPrices { get; set; }
        public CenterLocationResponseModel CenterLocation { get; set; }
        public virtual ICollection<CenterOperatingHoursResponseModel> CenterOperatingHours { get; set; }
    }
}