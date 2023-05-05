//using GoogleMaps.LocationServices;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class CenterRepository : RepositoryBase<Center>, ICenterRepository
    {
        public CenterRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task ActivateCenter(int id)
        {
            try
            {
                var center = this.DbContext.Centers.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Centers.Attach(center);
                center.Status = "Active";
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeactivateCenter(int id)
        {
            try
            {
                var center = this.DbContext.Centers.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Centers.Attach(center);
                center.Status = "Inactive";
                center.LastDeactivate = DateTime.Now;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<Center> SortCenterByLocation()
        {
            return null;
        }

        public new async Task<IEnumerable<Center>> GetAll()
        {
            var data = await this._dbContext.Centers
                .Include(center => center.Location)
                .ThenInclude(location => location.Ward)
                .ThenInclude(ward => ward.District)
                .Include(center => center.OperatingHours)
                .ThenInclude(oh => oh.DaysOfWeek)
                .Include(center => center.Services)
                .ThenInclude(service => service.Category)
                .Include(center => center.Services)
                .ThenInclude(service => service.ServicePrices)
                .Include(center => center.DeliveryPriceCharts)
                .Include(center => center.staff)
                .ThenInclude(staff => staff.Account)
                .ToListAsync();
            return data;
        }

        public async Task<IEnumerable<Center>> GetAllCenters()
        {
            var data = await this._dbContext.Centers
                .Include(center => center.Location)
                .ThenInclude(location => location.Ward)
                .ThenInclude(ward => ward.District)
                .Include(center => center.OperatingHours)
                .ThenInclude(oh => oh.DaysOfWeek)
                .Include(center => center.Services)
                .ThenInclude(service => service.Category)
                .ToListAsync();
            return data;
        }

        public new async Task<Center> GetById(int id)
        {
            var data = await this._dbContext.Centers
                .Include(center => center.Location)
                .ThenInclude(location => location.Ward)
                .ThenInclude(ward => ward.District)
                .Include(center => center.OperatingHours)
                .ThenInclude(oh => oh.DaysOfWeek)
                .Include(center => center.Services)
                .ThenInclude(service => service.Category)
                .Include(center => center.Services)
                .ThenInclude(service => service.ServicePrices)
                .Include(center => center.DeliveryPriceCharts)
                .Include(center => center.AdditionServices)
                .Include(center => center.CenterGalleries)
                .Include(center => center.Feedbacks)
                //.ThenInclude(service => service.OrderDetail)
                .Include(center => center.Resourses)
                .Include(center => center.Promotions)
                .FirstOrDefaultAsync(center => center.Id == id);
            return data;
        }


        public async Task<Center> GetMyCenter(int id)
        {
            var data = await this._dbContext.Centers
                .Where(center => center.Id == id)
                .Include(center => center.Location)//
                .ThenInclude(location => location.Ward)//
                .ThenInclude(ward => ward.District)//
                .Include(center => center.OperatingHours)//
                .ThenInclude(oh => oh.DaysOfWeek)//
                //.Include(center => center.Services)
                //.ThenInclude(service => service.Category)
                //.Include(center => center.Services)
                //.ThenInclude(service => service.ServicePrices)
                .Include(center => center.DeliveryPriceCharts)//
                .Include(center => center.AdditionServices)//
                .Include(center => center.CenterGalleries)//
                //.Include(center => center.Feedbacks)
                //.ThenInclude(service => service.OrderDetail)
                .Include(center => center.Resourses)//
                //.Include(center => center.Promotions)
                .FirstOrDefaultAsync();
            return data;
        }

        public async Task<Center> GetByIdLightWeight(int id)
        {
            var data = await _dbContext.Centers
                .FirstOrDefaultAsync(center => center.Id == id);
            return data;
        }
        
        public async Task<Center> GetByIdIncludeAddress(int id)
        {
            var data = await _dbContext.Centers
                .Where(center => center.Id == id)
                .Include(center => center.Location)
                .ThenInclude(location => location.Ward)
                .ThenInclude(ward => ward.District)
                .FirstOrDefaultAsync();
            return data;
        }

        public async Task<Center> GetByIdToCreateOrder(int id)
        {
            var data = await this._dbContext.Centers
                .Include(center => center.Location)
                .Include(center => center.OperatingHours)
                .FirstOrDefaultAsync(center => center.Id == id);
            return data;
        }

        public async Task<Center> GetDetailByIdLightWeight(int id)
        {
            var data = await _dbContext.Centers
                .Where(center => center.Id == id)
                .Include(center => center.Location)
                .ThenInclude(location => location.Ward)
                .ThenInclude(ward => ward.District)
                .Include(center => center.OperatingHours)
                .ThenInclude(oh => oh.DaysOfWeek)
                .Include(center => center.Services)
                .ThenInclude(service => service.Category)
                .Include(center => center.Services)
                .ThenInclude(service => service.ServicePrices)
                .Include(center => center.DeliveryPriceCharts)
                .FirstOrDefaultAsync();
            //.Include(center => center.Promotions)
            return data;
        }

        public async Task<Center> GetCenterOperatingTimes(int id)
        {
            var data = await _dbContext.Centers
                .Where(center => center.Id == id)
                .Include(center => center.OperatingHours)
                .ThenInclude(oh => oh.DaysOfWeek)
                .FirstOrDefaultAsync();
            //.Include(center => center.Promotions)
            return data;
        }

        public async Task<Center> GetByIdAdminDetail(int id)
        {
            var data = await this._dbContext.Centers
                    .Include(center => center.Location)
                        .ThenInclude(location => location.Ward)
                            .ThenInclude(ward => ward.District)
                    .Include(center => center.staff)
                        .ThenInclude(staff => staff.Account)
                    .Include(center => center.Feedbacks)
                    .Include(center => center.Services)
                        .ThenInclude(service => service.ServicePrices)
                    .Include(center => center.Services)
                        .ThenInclude(service => service.Category)
                    .FirstOrDefaultAsync(center => center.Id == id);
            return data;
        }

        public async Task<Center> GetByIdToCalculateDeliveryPrice(int id)
        {
            var data = await this._dbContext.Centers
                .Include(center => center.Location)
                .Include(center => center.DeliveryPriceCharts)
                .FirstOrDefaultAsync(center => center.Id == id);
            return data;
        }

        public new async Task<Center> GetByIdWithWallet(int id)
        {
            var data = await this._dbContext.Centers
                .Where(center => center.Id == id)
                .Include(center => center.Wallet)
                .ThenInclude(wallet => wallet.Transactions)
                .Include(center => center.Wallet)
                .ThenInclude(wallet => wallet.WalletTransactionFromWallets)
                .Include(center => center.Wallet)
                .ThenInclude(wallet => wallet.WalletTransactionToWallets)
                .FirstOrDefaultAsync();
            return data;
        }
        public async Task<string> CloseCenter(int id)
        {
            var data = await this._dbContext.Centers
                .Where(center => center.Id == id)
                .Select(center => new
                {
                    Orders = center.Services
                        .SelectMany(service => service.OrderDetails)
                        .Select(orderDetail => orderDetail.Order)
                        .Select(order => new { order.Id, order.Status })
                })
                .FirstOrDefaultAsync();

            if (data.Orders.Any(order => order.Status.Trim().ToLower() == "processing" || order.Status.Trim().ToLower() == "ready"
                                        || order.Status.Trim().ToLower() == "received" || order.Status.Trim().ToLower() == "pending" 
                                        || order.Status.Trim().ToLower() == "confirmed"))
            {
                return "one or more orders is already being processed";
            } else
            {
                var centerClose = await this._dbContext.Centers
                .Where(center => center.Id == id)
                .FirstOrDefaultAsync();
                centerClose.Status = "Closed";
                var staffs = await this._dbContext.Staffs
                        .Where(staff => staff.CenterId == id)
                        .ToListAsync();
                foreach (var item in staffs)
                {
                    item.CenterId = null;
                    item.IsManager = false;
                }
                await this._dbContext.SaveChangesAsync();
                return "success";
            }
            
        }
    }
}