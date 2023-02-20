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

        public Task Add(Center center)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Center>> GetAll()
        {
            return _centerRepository.Get();
        }

        public IEnumerable<Center> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
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

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task Update(Center center)
        {
            throw new NotImplementedException();
        }
    }
}
