using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Washouse.Model.ViewModel;

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
                .Include(order => order.Deliveries)
                .ThenInclude(delivery => delivery.Location)
                .ThenInclude(location => location.Ward)
                .ThenInclude(ward => ward.District)
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
                                        .ThenInclude(service => service.ServicePrices)
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

        public async Task<IEnumerable<Order>> GetOrdersOfCustomer(int customerId, string customerMobile)
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
                    .Where(order => order.CustomerId == customerId || order.CustomerMobile.Trim().Equals(customerMobile))
                    .ToListAsync();
            return data;
        }

        public async Task<StaffStatisticModel> GetStaffStatistics(int centerId)
        {
            var orders = (from o in _dbContext.Orders
                                 join od in _dbContext.OrderDetails on o.Id equals od.OrderId
                                 join s in _dbContext.Services on od.ServiceId equals s.Id
                          where s.CenterId == centerId && o.CreatedDate.Value.Date < DateTime.Now.Date.AddDays(1) && o.CreatedDate.Value.Date > DateTime.Now.Date.AddDays(-7)
                                 select o).Distinct();
            var orders_delivery = (from o in _dbContext.Orders
                          join od in _dbContext.OrderDetails on o.Id equals od.OrderId
                          join s in _dbContext.Services on od.ServiceId equals s.Id
                          join d in _dbContext.Deliveries on o.Id equals d.OrderId
                          where s.CenterId == centerId && o.CreatedDate.Value.Date < DateTime.Now.Date.AddDays(1) && o.CreatedDate.Value.Date > DateTime.Now.Date.AddDays(-7)
                                && (d.Status.Trim().ToLower() == "pending" || d.Status.Trim().ToLower() == "delivering")
                          select o).Distinct();
            var orderOverview = new OrderOverview
                                {
                                    NumOfPendingOrder = orders.Count(o => o.Status.ToLower().Trim() == "pending"),
                                    NumOfProcessingOrder = orders.Count(o => o.Status.ToLower().Trim() == "processing" || o.Status.ToLower().Trim() == "received"
                                                                    || o.Status.ToLower().Trim() == "confirmed"),
                                    NumOfReadyOrder = orders.Count(o => o.Status.ToLower().Trim() == "ready"),
                                    NumOfPendingDeliveryOrder = orders_delivery.Count(),
                                    NumOfCompletedOrder = orders.Count(o => o.Status.ToLower().Trim() == "completed"),
                                    NumOfCancelledOrder = orders.Count(o => o.Status.ToLower().Trim() == "cancelled"),
                                };
            // Create a list of all dates you want to include
            var dateList = Enumerable.Range(-6, 7)
                .Select(offset => DateTime.Now.AddDays(offset).Date)
                .Select(date => date.ToString("dd-MM-yyyy"))
                .ToList();

            // Query daily statistics as before
            var dailyStatistics = from o in (
                          from o in _dbContext.Orders
                          join od in _dbContext.OrderDetails on o.Id equals od.OrderId
                          join s in _dbContext.Services on od.ServiceId equals s.Id
                          where s.CenterId == centerId && o.CreatedDate.Value.Date < DateTime.Now.Date.AddDays(1) && o.CreatedDate.Value.Date > DateTime.Now.Date.AddDays(-7)
                          select o
                      ).Distinct()
                                  join p in _dbContext.Payments on o.Id equals p.OrderId into pg
                                  from payment in pg.DefaultIfEmpty()
                                  group new { o, payment } by o.CreatedDate.Value.Date into g
                                  select new DailyStatistic
                                  {
                                      Day = g.Key.ToString("dd-MM-yyyy"),
                                      TotalOrder = g.Count(),
                                      SuccessfulOrder = g.Count(o => o.o.Status.ToLower().Trim() == "completed"),
                                      CancelledOrder = g.Count(o => o.o.Status.ToLower().Trim() == "cancelled"),
                                      Revenue = g.Sum(x => x.payment != null ? x.payment.Total : 0)
                                  };

            // Left join the two lists on the date field to get all dates with zero successful/cancelled orders
            var result = from date in dateList
                         join ds in dailyStatistics on date equals ds.Day into gj
                         from subDs in gj.DefaultIfEmpty()
                         select new DailyStatistic
                         {
                             Day = date,
                             TotalOrder = subDs == null ? 0 : subDs.TotalOrder,
                             SuccessfulOrder = subDs == null ? 0 : subDs.SuccessfulOrder,
                             CancelledOrder = subDs == null ? 0 : subDs.CancelledOrder,
                             Revenue = subDs == null ? 0 : subDs.Revenue
                         };
            return new StaffStatisticModel
            {
                orderOverview = orderOverview,
                dailystatistics = result.ToList(),
            };
        }
    }
}