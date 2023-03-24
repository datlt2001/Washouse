using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Abstract;

namespace Washouse.Model.Models
{
    public partial class DeliveryPriceChart : Auditable
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public decimal? MaxDistance { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
        public bool Status { get; set; }

        public virtual Center Center { get; set; }
    }
}
