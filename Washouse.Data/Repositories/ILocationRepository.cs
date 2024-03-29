﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<Location> GetLocationOfACenter(int centerId);
        Task<Location> GetByIdIncludeWardDistrict(int id);
        Task<Location> GetBySearch(Location location);
        Task<Location> GetByIdCheckExistCenter(int id);
    }
}
