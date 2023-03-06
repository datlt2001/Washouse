using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class ServiceRequestService : IServiceRequestService
    {
        IServiceRequestRepository _serviceRequestRepository;
        IUnitOfWork _unitOfWork;

        public ServiceRequestService(IServiceRequestRepository serviceRequestRepository, IUnitOfWork unitOfWork)
        {
            this._serviceRequestRepository = serviceRequestRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task Add(ServiceRequest serviceRequest)
        {
            await _serviceRequestRepository.Add(serviceRequest);
        }

        public Task<IEnumerable<ServiceRequest>> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServiceRequest> GetAllPaging(int page, int pageSize, out int totalRow)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceRequest> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task Update(ServiceRequest serviceRequest)
        {
            throw new NotImplementedException();
        }
    }
}
