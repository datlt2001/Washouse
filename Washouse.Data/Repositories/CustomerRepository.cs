using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        public CustomerRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
        public async Task DeactivateCustomer(int id)
        {
            try
            {

                var customer = this.DbContext.Customers.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Customers.Attach(customer);
                customer.Status = false;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ActivateCustomer(int id)
        {
            try
            {

                var customer = this.DbContext.Customers.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Customers.Attach(customer);
                customer.Status = true;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
