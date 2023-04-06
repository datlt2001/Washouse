using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface ICustomerRepository :IRepository<Customer>
    {
        public Task DeactivateCustomer(int id);

        public Task ActivateCustomer(int id);
        Task<IEnumerable<Customer>> CustomersOfCenter(int centerId);
    }
}
