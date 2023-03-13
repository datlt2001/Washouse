using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Resourse
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public string ResourceName { get; set; }
        public string Alias { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal? WashCapacity { get; set; }
        public decimal? DryCapacity { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public virtual Center Center { get; set; }
    }
}
