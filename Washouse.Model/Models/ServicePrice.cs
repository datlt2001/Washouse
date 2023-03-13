using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class ServicePrice
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal Price { get; set; }

        public virtual Service Service { get; set; }
    }
}
