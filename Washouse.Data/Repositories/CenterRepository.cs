//using GoogleMaps.LocationServices;
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
    public class CenterRepository : RepositoryBase<Center>, ICenterRepository
    {
        public CenterRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public Task ActivateCategory(int id)
        {
            throw new NotImplementedException();
        }

        public Task DeactivateCategory(int id)
        {
            throw new NotImplementedException();
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
                    .ToListAsync(); ;
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
                    .FirstOrDefaultAsync(center => center.Id == id);
            return data;
        }
    }
}
