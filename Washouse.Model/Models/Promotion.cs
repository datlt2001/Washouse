using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Promotion
    {
        public Promotion()
        {
            Payments = new HashSet<Payment>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public int? UseTimes { get; set; }
        public int CenterId { get; set; }

        public virtual Center Center { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
