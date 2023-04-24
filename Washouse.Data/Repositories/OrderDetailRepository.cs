using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class OrderDetailRepository : RepositoryBase<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task<IEnumerable<OrderDetail>> GetByOrderId(string orderId)
        {
            return _dbContext.OrderDetails
                .Where(o => Equals(o.OrderId, orderId))
                .Include(o => o.Service);
        }
    }
}