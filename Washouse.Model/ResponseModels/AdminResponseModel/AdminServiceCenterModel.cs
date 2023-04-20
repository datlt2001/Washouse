using System.Collections.Generic;
using Washouse.Model.Models;
using Washouse.Model.ViewModel;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class AdminServiceCenterModel
    {
        public AdminServiceCenterModel() {
            Prices = new HashSet<ServicePriceViewModel>();
        }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Alias { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool PriceType { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public string Unit { get; set; }
        public decimal Rate { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public virtual ICollection<ServicePriceViewModel> Prices { get; set; }
    }
}