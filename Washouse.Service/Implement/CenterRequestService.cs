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
    public class CenterRequestService : ICenterRequestService
    {
        ICenterRepository _centerRepository;
        ICenterRequestRepository _centerRequestRepository;
        IUnitOfWork _unitOfWork;

        public CenterRequestService(ICenterRepository centerRepository, IUnitOfWork unitOfWork, ICenterRequestRepository centerRequestRepository)
        {
            this._centerRepository = centerRepository;
            this._unitOfWork = unitOfWork;
            _centerRequestRepository = centerRequestRepository;
        }

        public async Task Add(CenterRequest serviceRequest)
        {
            await _centerRequestRepository.Add(serviceRequest);
        }

        public Task<IEnumerable<CenterRequest>> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CenterRequest> GetAllPaging(int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public Task<CenterRequest> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task Update(CenterRequest serviceRequest)
        {
            throw new NotImplementedException();
        }
    }
}
