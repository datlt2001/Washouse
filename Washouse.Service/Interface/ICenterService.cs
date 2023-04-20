﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface ICenterService
    {
        Task<IEnumerable<Center>> GetAll();
        Task Add(Center center);

        Task Update(Center center);

        //Task Delete(int id);

        IEnumerable<Center> GetAllPaging(int page, int pageSize, out int totalRow);

        IEnumerable<Center> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow);

        IEnumerable<Center> GetAllBySearchKeyPaging(string searchKey, int page, int pageSize, out int totalRow);

        Task<Center> GetById(int id);
        Task<Center> GetMyCenter(int id);

        IEnumerable<Center> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow);

        void SaveChanges();
        Task DeactivateCenter(int id);
        Task ActivateCenter(int id);

        Task<Center> GetByIdLightWeight(int id);
        Task<Center> GetDetailByIdLightWeight(int id);
        Task<Center> GetByIdToCreateOrder(int id);
        Task<Center> GetByIdAdminDetail(int id);
    }
}