﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface ICenterRepository : IRepository<Center>
    {
        IEnumerable<Center> SortCenterByLocation();
        Task<IEnumerable<Center>> GetAllCenters();
        Task ActivateCenter(int id);

        Task DeactivateCenter(int id);

        Task<string> CloseCenter(int id);
        Task<Center> GetByIdLightWeight(int id);
        Task<Center> GetByIdWithWallet(int id);
        Task<Center> GetByIdToCalculateDeliveryPrice(int id);
        Task<Center> GetDetailByIdLightWeight(int id);
        Task<Center> GetCenterOperatingTimes(int id);
        Task<Center> GetMyCenter(int id);
        Task<Center> GetByIdToCreateOrder(int id);
        Task<Center> GetByIdAdminDetail(int id);
        Task<Center> GetByIdIncludeAddress(int id);
    }
}