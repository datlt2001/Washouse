using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequest>> GetAll();
        Task Add(ServiceRequest serviceRequest);
        Task Update(ServiceRequest serviceRequest);
        IEnumerable<ServiceRequest> GetAllPaging(int page, int pageSize, out int totalRow);
        Task<ServiceRequest> GetById(int id);
        void SaveChanges();
    }
}
