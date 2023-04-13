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
            OrderTrackings = new HashSet<OrderTracking>();
            Feedbacks = new HashSet<Feedback>();
        }

        public string Id { get; set; }
        public string CustomerName { get; set; }
        public int LocationId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerMessage { get; set; }
        public int CustomerId { get; set; }
        public int DeliveryType { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public DateTime? PreferredDropoffTime { get; set; }
        public DateTime? PreferredDeliverTime { get; set; }
        public string Status { get; set; }
        public string CancelReasonByStaff { get; set; }
        public string CancelReasonByCustomer { get; set; }

        public virtual Location Location { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Delivery> Deliveries { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<OrderTracking> OrderTrackings { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
