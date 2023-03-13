using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class CenterService : ICenterService
    {
        ICenterRepository _centerRepository;
        IUnitOfWork _unitOfWork;

        public CenterService(ICenterRepository centerRepository, IUnitOfWork unitOfWork)
        {
            this._centerRepository = centerRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Center>> GetAll()
        {
            return await _centerRepository.GetAll();
        }

        public IEnumerable<Center> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<Center> GetAllBySearchKeyPaging(string searchKey, int page, int pageSize, out int totalRow)
        {
            var query = _centerRepository.GetMulti(x => (x.Status == "active") && (x.CenterName.Contains(searchKey) || x.Alias.Contains(searchKey)));
            totalRow = query.Count();

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }
        public IEnumerable<Center> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Center> GetAllPaging(int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public async Task<Center> GetById(int id)
        {
            return await _centerRepository.GetById(id);
        }

        public async Task Add(Center center)
        {
            await _centerRepository.Add(center);
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task Update(Center center)
        {
            throw new NotImplementedException();
        }

        public Task ActivateCenter(int id)
        {
            //await _centerRepository.ActivateCategory(id);
            throw new NotImplementedException();
        }

        public Task DeactivateCenter(int id)
        {
            //await _centerRepository.DeactivateCenter(id);
            throw new NotImplementedException();
        }
    }
}
