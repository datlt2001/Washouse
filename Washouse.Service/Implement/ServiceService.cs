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
    public class ServiceService : IServiceService
    {
        IServiceRepository _serviceRepository;
        IUnitOfWork _unitOfWork;

        public ServiceService(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
        {
            this._serviceRepository = serviceRepository;
            this._unitOfWork = unitOfWork;
        }
        public Task Add(Model.Models.Service center)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Model.Models.Service>> GetAll()
        {
            return _serviceRepository.Get();
        }

        public IEnumerable<Model.Models.Service> GetAllByCategoryPaging(int categoryId, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.Models.Service> GetAllByTagPaging(string tag, int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Model.Models.Service> GetAllPaging(int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public async Task<Model.Models.Service> GetById(int id)
        {
            return await _serviceRepository.GetById(id);
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task Update(Model.Models.Service center)
        {
            throw new NotImplementedException();
        }

        public async Task DeactivateService(int id)
        {
            await _serviceRepository.DeactivateService(id);
        }
    }
}
