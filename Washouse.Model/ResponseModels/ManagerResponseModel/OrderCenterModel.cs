using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderCenterModel
    {
        public string OrderId { get; set; }
        public string OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalOrderPayment { get; set; }
        public string Status { get; set; }
        public List<OrderedServiceModel> OrderedServices { get; set; }
    }
}
