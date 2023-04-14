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
    public class UpdateServiceRequestModel
    {
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int? TimeEstimate { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public virtual ICollection<ServicePriceViewModel> ServicePrices { get; set; }
    }
}
