using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;

namespace Washouse.Service
{
    public class ServiceCategoryService : IServiceCategoryService
    {
        private IServiceCategoryRepository _ServiceCategoryRepository;
        private IUnitOfWork _unitOfWork;

        public ServiceCategoryService(IServiceCategoryRepository ServiceCategoryRepository, IUnitOfWork unitOfWork)
        {
            this._ServiceCategoryRepository = ServiceCategoryRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task Add(Category ServiceCategory)
        {
             await _ServiceCategoryRepository.Add(ServiceCategory);
        }

        //public Category Delete(long id)
        //{
        //    return await _ServiceCategoryRepository.Delete(id);
        //}

        public IEnumerable<Category> GetAll()
        {
            return _ServiceCategoryRepository.Get();
        }

        //public IEnumerable<Category> GetAll(string keyword)
        //{
        //    if (!string.IsNullOrEmpty(keyword)) return _ServiceCategoryRepository.GetMulti(x => x.CategoryName.Contains(keyword));
        //    else
        //        return _ServiceCategoryRepository.GetAll();
        //}

        //public IEnumerable<Category> GetAllByParentId(int id)
        //{
        //    return _ServiceCategoryRepository.GetMulti(x => x.Status && x.ParentId == id);
        //}

        //public void Save()
        //{
        //    _unitOfWork.Commit();
        //}

        public async Task<Category> GetById(int id)
        {
           return await _ServiceCategoryRepository.GetById(id);
        }

        public IEnumerable<Category> GetCategoryByParentId(int id)
        {
            return  _ServiceCategoryRepository.GetCategoryByParentId(id);
        }

        public IEnumerable<Category> GetAllParentCategory()
        {
            return  _ServiceCategoryRepository.GetAllParentCategory();
        }

        public async Task Update(Category ServiceCategory)
        {
            await _ServiceCategoryRepository.Update(ServiceCategory);
        }

        public async Task ActivateCategory(int id)
        {
            await _ServiceCategoryRepository.ActivateCategory(id);
        }

        public async Task DeactivateCategory(int id)
        {
            await _ServiceCategoryRepository.DeactivateCategory(id);
        }
    }
}
