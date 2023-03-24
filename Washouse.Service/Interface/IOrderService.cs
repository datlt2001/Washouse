using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IOrderService
    {
        Task<Order> Create(Order order, List<OrderDetail> orderDetails, List<OrderAddition> orderAdditions);
        void UpdateStatus(int orderId);
        void Save();
    }
}
