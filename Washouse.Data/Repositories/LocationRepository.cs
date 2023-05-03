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
    public class LocationRepository : RepositoryBase<Location>, ILocationRepository
    {
        public LocationRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task<Location> GetLocationOfACenter(int centerId)
        {
            return await this.DbContext.Locations.FindAsync(centerId);
        }

        public new async Task<Location> GetById(int id)
        {
            var data = await this._dbContext.Locations
                .Where(location => location.Id == id)
                    .Include(location => location.Centers)
                    .Include(location => location.Ward)
                        .ThenInclude(ward => ward.District)
                    .FirstOrDefaultAsync();
            return data;
        }

        public async Task<Location> GetByIdIncludeWardDistrict(int id)
        {
            var data = await this._dbContext.Locations
                    .Where(location => location.Id == id)
                    .Include(location => location.Ward)
                        .ThenInclude(ward => ward.District)
                    .FirstOrDefaultAsync();
            return data;
        }
    }
}
