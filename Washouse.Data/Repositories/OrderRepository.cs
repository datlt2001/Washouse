using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class OrderRepository : RepositoryBase<Order>, IOrderRepository
    {
        public OrderRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public new async Task<IEnumerable<Order>> GetAll()
        {
            var data = await this._dbContext.Orders
                    .Include(order => order.OrderDetails)
                        .ThenInclude(orderDetail => orderDetail.Service)
                            .ThenInclude(service => service.Center)
                    .ToListAsync();
            return data;
        }


        public async Task<IEnumerable<Order>> GetOrdersOfCenter(int centerId)
        {
            var ordersAtCenter = await this._dbContext.Orders
                                .Include(order => order.Payments)
                                .Include(order => order.OrderDetails)
                                    .Where(o => o.OrderDetails
                                    .Any(od => od.Service.Center.Id == centerId))
                                .ToListAsync();
            return ordersAtCenter;
        }
    }
}
