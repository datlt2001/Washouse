using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class AdditionService : Auditable
    {
        public AdditionService()
        {
            OrderAdditions = new HashSet<OrderAddition>();
        }

        public int Id { get; set; }
        public string AdditionName { get; set; }
        public string Alias { get; set; }
        public decimal? MaxDistance { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
        public bool Status { get; set; }
        public int CenterId { get; set; }

        public virtual Center Center { get; set; }
        public virtual ICollection<OrderAddition> OrderAdditions { get; set; }
    }
}
