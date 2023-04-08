using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class OrderDetailTrackingRepository : RepositoryBase<OrderDetailTracking>, IOrderDetailTrackingRepository
    {
        public OrderDetailTrackingRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}
