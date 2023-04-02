using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ViewModel
{
    public class CartItemModel
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public decimal Price { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? MinPrice { get; set; }
        public virtual ICollection<ServicePriceViewModel> PriceChart { get; set; }
        public string Unit { get; set; }
    }
}
