﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface ICenterRepository : IRepository<Center>
    {
        IEnumerable<Center> SortCenterByLocation();
        Task ActivateCenter(int id);

        Task DeactivateCenter(int id);

        Task<Center> GetByIdLightWeight(int id);
        Task<Center> GetMyCenter(int id);
        Task<Center> GetByIdToCreateOrder(int id);
        Task<Center> GetByIdAdminDetail(int id);

    }
}