//using GoogleMaps.LocationServices;

using System;
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
    }
}