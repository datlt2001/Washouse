using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.ViewModel;

namespace Washouse.Model.RequestModels
{
    public class ServiceRequestModel
    {
        public string ServiceName { get; set; }
        public string Alias { get; set; }
        public int ServiceCategory { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceImage { get; set; }
        public int TimeEstimate { get; set; }
        public string Unit { get; set; }
        public decimal Rate { get; set; }
        public bool PriceType { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public virtual ICollection<string> ServiceGalleries { get; set; }
        public virtual ICollection<ServicePriceViewModel> Prices { get; set; }
    }
}
