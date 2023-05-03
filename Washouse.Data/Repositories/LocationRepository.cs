using Microsoft.CodeAnalysis;
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
    public class LocationRepository : RepositoryBase<Model.Models.Location>, ILocationRepository
    {
        public LocationRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task<Model.Models.Location> GetLocationOfACenter(int centerId)
        {
            return await this.DbContext.Locations.FindAsync(centerId);
        }

        public async Task<Model.Models.Location> GetById(int id)
        {
            var data = await this._dbContext.Locations
                .Where(location => location.Id == id)
                    .Include(location => location.Centers)
                    .Include(location => location.Ward)
                        .ThenInclude(ward => ward.District)
                    .FirstOrDefaultAsync();
            return data;
        }
        
        public async Task<Model.Models.Location> GetBySearch(Model.Models.Location location)
        {
            var item = await _dbContext.Locations
                .Where(x => x.Latitude != null && x.Longitude != null
                && location.Latitude != null && location.Longitude != null
                            && x.WardId == location.WardId
                && (x.AddressString.ToLower().Contains(location.AddressString.ToLower()) || location.AddressString.ToLower().Contains(x.AddressString.ToLower()))
                && ((x.Latitude - location.Latitude) < (decimal)0.05)
                && ((x.Longitude - location.Longitude) < (decimal)0.05))
                .FirstOrDefaultAsync();

            return item;
        }


        public async Task<Model.Models.Location> GetByIdCheckExistCenter(int id)
        {
            var data = await this._dbContext.Locations
                .Where(location => location.Id == id)
                    .Include(location => location.Centers)
                    .FirstOrDefaultAsync();
            return data;
        }

        public async Task<Model.Models.Location> GetByIdIncludeWardDistrict(int id)
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
