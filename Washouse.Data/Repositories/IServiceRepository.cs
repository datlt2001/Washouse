using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task DeactivateService(int id);

        public IEnumerable<Service> GetServicesByCategory(int cateID);
        public Task<IEnumerable<Service>> GetAllByCenterId(int centerId);

        Task<Service> GetByIdToCreateOrder(int id);
    }
}