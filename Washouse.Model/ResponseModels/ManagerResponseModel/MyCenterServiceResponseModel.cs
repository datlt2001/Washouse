using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ViewModel;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class MyCenterServiceResponseModel
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string ServiceName { get; set; }
        public bool PriceType { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public string Unit { get; set; }
        public decimal Rate { get; set; }
        public virtual ICollection<ServicePriceViewModel> Prices { get; set; }
        public int? TimeEstimate { get; set; }
    }
}
