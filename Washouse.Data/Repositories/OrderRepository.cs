using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
