using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class OrderAddition
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int AdditionId { get; set; }
        public decimal Price { get; set; }

        public virtual AdditionService Addition { get; set; }
        public virtual Order Order { get; set; }
    }
}
