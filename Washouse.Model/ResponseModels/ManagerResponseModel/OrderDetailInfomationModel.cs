using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderDetailInfomationModel
    {
        public int OrderDetailId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategory { get; set; }
        public decimal? Measurement { get; set; }
        public string Unit { get; set; }
        public string Image { get; set; }
        public string CustomerNote { get; set; }
        public string StaffNote { get; set; }
        public string Status { get; set; }
        public decimal? Price { get; set; }
        public decimal? UnitPrice { get; set; }
        public List<OrderDetailTrackingModel> OrderDetailTrackings { get; set; }
    }
}
