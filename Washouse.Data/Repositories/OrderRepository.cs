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
                                    .ThenInclude(od => od.Service)
                                        .ThenInclude(service => service.Category)
                    .Include(order => order.OrderDetails)
                                    .ThenInclude(od => od.Service)
                                        .ThenInclude(service => service.Center)
                    .Include(order => order.Payments)
                                    .ThenInclude(od => od.PromoCodeNavigation)
                    .Include(order => order.Deliveries)
                    .Include(order => order.OrderTrackings)
                    .Include(order => order.OrderDetails)
                                    .ThenInclude(od => od.OrderDetailTrackings)
                    .Include(order => order.Customer)
                    .ToListAsync();
            return data;
        }


        public async Task<IEnumerable<Order>> GetOrdersOfCenter(int centerId)
        {
            var ordersAtCenter = await this._dbContext.Orders
                                .Include(order => order.Payments)
                                .Include(order => order.OrderDetails)
                                    .ThenInclude(od => od.Service)
                                        .ThenInclude(service => service.Category)
                                    .Where(o => o.OrderDetails
                                    .Any(od => od.Service.Center.Id == centerId))
                                .ToListAsync();
            return ordersAtCenter;
        }

        public async Task<Order> GetOrderById(string id)
        {
            var data = await this._dbContext.Orders
                    .Include(order => order.OrderDetails)
                                    .ThenInclude(od => od.Service)
                                        .ThenInclude(service => service.Category)
                    .Include(order => order.OrderDetails)
                                    .ThenInclude(od => od.Service)
                                        .ThenInclude(service => service.Center)
                    .Include(order => order.Payments)
                                    .ThenInclude(od => od.PromoCodeNavigation)
                    .Include(order => order.Payments)
                                    .ThenInclude(od => od.WalletTransactions)
                    .Include(order => order.Deliveries)
                    .Include(order => order.OrderTrackings)
                    .Include(order => order.OrderDetails)
                                    .ThenInclude(od => od.OrderDetailTrackings)
                    .Include(order => order.Customer)
                    .Include(order => order.Location)
                        .ThenInclude(location => location.Ward)
                            .ThenInclude(ward => ward.District)
                    .FirstOrDefaultAsync(order => order.Id == id);
            return data;
        }
    }
}
