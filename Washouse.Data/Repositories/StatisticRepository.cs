using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels.AdminResponseModel;
using Washouse.Model.ResponseModels.ManagerResponseModel;

namespace Washouse.Data.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        WashouseDbContext _dbContext = new WashouseDbContext();
        public async Task<AdminStatisticResponseModel> GetAdminStatistic(string fromDate, string toDate)
        {
            try
            {
                var response = new AdminStatisticResponseModel();
                DateTime? _fromDate = null;
                if (fromDate != null) 
                {
                    _fromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                } 
                DateTime? _toDate = null;
                if (toDate != null) 
                {
                    _toDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                if (fromDate == null && toDate == null)
                {
                    _toDate = DateTime.Now.Date;
                    _fromDate = DateTime.Now.AddDays(-6).Date;
                }
                var customerStatistic = new CustomerStatistic();
                var customer = this._dbContext.Customers
                    .Where(c => c.CreatedDate != null)
                    .Select(c => new {c.CreatedDate })
                    .ToList();
                customerStatistic.NumberOfNewCustomersToday = customer.Count(cus => cus.CreatedDate.Value.Date == DateTime.Now.Date);
                customerStatistic.NumberOfNewCustomersYesterday = customer.Count(cus => cus.CreatedDate.Value.Date == DateTime.Now.AddDays(-1).Date);
                // Compute the number of new customers daily within the specified date range
                var customerCounts = this._dbContext.Customers
                    .Where(c => c.CreatedDate != null && c.CreatedDate.Value.Date >= _fromDate.Value.Date && c.CreatedDate.Value.Date <= _toDate.Value.Date)
                    .GroupBy(c => c.CreatedDate.Value.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToList();

                // Create a list of dictionaries to store the customer counts by date
                var customerCountsByDate = new List<Dictionary<string, int>>();

                // Initialize the date range and set the initial count to zero for each date
                var currentDate = _fromDate.Value.Date;
                var endDate = _toDate.Value.Date;
                var customerCountDict = new Dictionary<string, int>();
                while (currentDate <= endDate)
                {
                    customerCountDict[currentDate.ToString("dd-MM-yyyy")] = 0;
                    currentDate = currentDate.AddDays(1);
                }

                // Update the customer count for each date in the dictionary
                foreach (var customerCount in customerCounts)
                {
                    customerCountDict[customerCount.Date.ToString("dd-MM-yyyy")] = customerCount.Count;
                }

                // Add the customer count dictionary to the response
                customerCountsByDate.Add(customerCountDict);

                // Set the NumberOfNewCustomersDaily property in the response
                customerStatistic.NumberOfNewCustomersDaily = customerCountsByDate;

                response.CustomerStatistic = customerStatistic;

                var postStatistic = new PostStatistic();

                var postDates = this._dbContext.Posts
                    .Select(p => p.CreatedDate.Date)
                    .ToList();

                postStatistic.NumberOfNewPostsToday = postDates.Count(date => date == DateTime.Now.Date);
                postStatistic.NumberOfNewPostsYesterday = postDates.Count(date => date == DateTime.Now.AddDays(-1).Date);

                response.PostStatistic = postStatistic;


                var centerStatistics = new CenterStatistic();

                var centers = this._dbContext.Centers
                    .Where(c => c.Status != null && c.CreatedDate != null)
                    .Select(c => new { c.Status, c.CreatedDate })
                    .ToList();

                centerStatistics.NumberOfPendingCenters = centers.Count(center => center.Status.Trim().ToLower().Equals("pending"));
                centerStatistics.NumberOfClosedCenters = centers.Count(center => center.Status.Trim().ToLower().Equals("closed"));
                centerStatistics.NumberOfActiveCenters = centers.Count(center => !center.Status.Trim().ToLower().Equals("closed"));
                centerStatistics.NumberOfNewCenterToday = centers.Count(center => center.CreatedDate.Value.Date == DateTime.Now.Date);
                centerStatistics.NumberOfNewCenterYesterday = centers.Count(center => center.CreatedDate.Value.Date == DateTime.Now.AddDays(-1).Date);
                response.CenterStatistic = centerStatistics;
                
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<StaffStatisticModel> GetManagerStatistic(int centerId, string fromDate, string toDate)
        {
            DateTime startDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(toDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

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
            var dateList = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset).ToString("dd-MM-yyyy"))
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
