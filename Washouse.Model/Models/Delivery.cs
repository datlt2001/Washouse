using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Delivery : Auditable
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string ShipperName { get; set; }
        public string ShipperPhone { get; set; }
        public int LocationId { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; }

        public virtual Location Location { get; set; }
        public virtual Order Order { get; set; }
    }
}
