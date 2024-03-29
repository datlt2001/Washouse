﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class WardRepository : RepositoryBase<Ward>, IWardRepository
    {
        public WardRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task<IEnumerable<Ward>> GetWardListByDistrictId(int DistrictId)
        {
            return await this._dbContext.Wards.Where(x => x.DistrictId == DistrictId).ToListAsync();
        }

        public async Task<Ward> GetWardById(int WardId)
        {
            var data = await this._dbContext.Wards
                    .Include(a => a.District)
                    .Where(ward => ward.Id == WardId)
                    .FirstOrDefaultAsync();
            return data;
        }

        public async Task<Ward> GetWardByName(string Name)
        {
            var data = await this._dbContext.Wards
                    .Include(a => a.District)
                    .Where(ward => ward.WardName == Name)
                    .FirstOrDefaultAsync();
            return data;
        }
    }
}
