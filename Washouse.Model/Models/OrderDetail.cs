using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class OrderDetail
    {
        public OrderDetail()
        {
            Feedbacks = new HashSet<Feedback>();
        }

        public int Id { get; set; }
        public string OrderId { get; set; }
        public int ServiceId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string CustomerNote { get; set; }
        public string StaffNote { get; set; }
        public virtual Order Order { get; set; }
        public virtual Service Service { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
