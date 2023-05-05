using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface ICenterRequestService
    {
        Task<IEnumerable<CenterRequest>> GetAll();
        Task Add(CenterRequest serviceRequest);
        Task Update(CenterRequest serviceRequest);
        IEnumerable<CenterRequest> GetAllPaging(int page, int pageSize, out int totalRow);
        Task<CenterRequest> GetById(int id);
        void SaveChanges();
        Task<IEnumerable<CenterRequest>> GetCenterRequests();
    }
}
