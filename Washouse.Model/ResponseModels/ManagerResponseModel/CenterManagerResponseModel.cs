using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class CenterManagerResponseModel
    {
        public CenterManagerResponseModel()
        {
            CenterOperatingHours = new HashSet<CenterOperatingHoursResponseModel>();
            CenterDeliveryPrices = new HashSet<CenterDeliveryPriceChartResponseModel>();
            AdditionServices = new HashSet<AdditionServiceCenterModel>();
            CenterGalleries = new HashSet<CenterGalleryModel>();
            CenterFeedbacks = new HashSet<FeedbackCenterModel>();
            CenterResourses = new HashSet<ResourseCenterModel>();
        }
        public int Id { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public string Phone { get; set; }
        public string CenterAddress { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public string TaxCode { get; set; }
        public string TaxRegistrationImage { get; set; }
        public bool MonthOff { get; set; }
        public bool HasDelivery { get; set; }
        public int LocationId { get; set; }

        public virtual ICollection<CenterDeliveryPriceChartResponseModel> CenterDeliveryPrices { get; set; }
        public CenterLocationResponseModel CenterLocation { get; set; }
        public virtual ICollection<CenterOperatingHoursResponseModel> CenterOperatingHours { get; set; }
        public virtual ICollection<AdditionServiceCenterModel> AdditionServices { get; set; }
        public virtual ICollection<CenterGalleryModel> CenterGalleries { get; set; }
        public virtual ICollection<FeedbackCenterModel> CenterFeedbacks { get; set; }
        public virtual ICollection<ResourseCenterModel> CenterResourses { get; set; }
    }
}
