using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class ServicePrice : Auditable
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public decimal MaxValue { get; set; }
        public decimal Price { get; set; }

        public virtual Service Service { get; set; }
    }
}
