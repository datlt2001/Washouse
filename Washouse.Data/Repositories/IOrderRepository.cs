using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels.ManagerResponseModel;

namespace Washouse.Data.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersOfCenter(int centerId);
        Task<Order> GetOrderById(string orderId);
        Task<Order> GetOrderWithPayment(string orderId);
        Task<StaffStatisticModel> GetStaffStatistics(int centerId);
        Task<IEnumerable<Order>> GetOrdersOfCustomer(int customerId, string customerMobile);
    }
}
