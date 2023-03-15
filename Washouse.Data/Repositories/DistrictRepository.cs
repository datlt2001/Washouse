using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class DistrictRepository : RepositoryBase<District>, IDistrictRepository
    {
        public DistrictRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task<District> GetDistrictByName(string name)
        {
            return await this.DbContext.Districts.SingleOrDefaultAsync(district => district.DistrictName.ToLower().Equals(name.ToLower()));
        }
    }
}
