using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Tracking : Auditable
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }

        public virtual Order Order { get; set; }
    }
}
