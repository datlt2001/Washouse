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

namespace Washouse.Data.Repositories
{
    public class StatisticRepository : RepositoryBase<Notification>, IStatisticRepository
    {
        public StatisticRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }


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
    }
}
