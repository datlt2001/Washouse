using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Order : Auditable
    {
        public Order()
        {
            Deliveries = new HashSet<Delivery>();
            Notifications = new HashSet<Notification>();
            OrderDetails = new HashSet<OrderDetail>();
            Payments = new HashSet<Payment>();
            Trackings = new HashSet<Tracking>();
        }

        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerMessage { get; set; }
        public int CustomerId { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public string Status { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<Delivery> Deliveries { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Tracking> Trackings { get; set; }
    }
}
