using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Abstract;

namespace Washouse.Model.Models
{
    public partial class OrderDetailTracking : Auditable
    {
        public int Id { get; set; }
        public int OrderDetailId { get; set; }
        public string Status { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
    }
}
