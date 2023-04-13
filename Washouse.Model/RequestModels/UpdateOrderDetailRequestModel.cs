using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class UpdateOrderDetailRequestModel
    {
        public decimal? Measurement { get; set; }
        public string StaffNote { get; set; }
    }
}
