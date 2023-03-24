using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CreateOrderRequestModel
    {
        public OrderRequestModel Order { get; set; }
        public ICollection<OrderDetailRequestModel> OrderDetails { get; set; }
    }
}
