using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface ICustomerService
    {
        public Task Add(Customer customer);

        public Task Update(Customer customer);

        IEnumerable<Customer> GetAll();

        public Task<Customer> GetById(int id);

        public Task<Customer> GetByPhone(string phone);

        public Task DeactivateCustomer(int id);

        public Task ActivateCustomer(int id);
    }
}
