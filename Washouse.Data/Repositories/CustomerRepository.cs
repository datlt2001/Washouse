using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Customer>> CustomersOfCenter(int centerId)
        {
            try
            {
                var customersWithOrders = await this.DbContext.Customers
                    .Where(c => c.Orders
                        .Any(o => o.OrderDetails
                            .Any(od => od.Service.Center.Id == centerId)))
                    .Include(cus => cus.AddressNavigation)
                        .ThenInclude(loc => loc.Ward)
                            .ThenInclude(ward => ward.District)
                    .Include(cus => cus.Account)
                    .ToListAsync();
                return customersWithOrders;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Customer> GetCustomerByAccID(int accountId)
        {
            try
            {
                var customer = await this.DbContext.Customers
                     .Include(cus => cus.Account)
                                    .ThenInclude(acc => acc.Wallet)
                    .SingleOrDefaultAsync(c => c.AccountId == accountId);
                    return customer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Customer> GetCustomerByAccIDLightWeight(int accountId)
        {
            try
            {
                var customer = await this.DbContext.Customers
                    .SingleOrDefaultAsync(c => c.AccountId == accountId);
                return customer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Customer> GetByPhone(string phone)
        {
            try
            {
                var customer = await this.DbContext.Customers                    
                    .SingleOrDefaultAsync(c => c.Phone == phone);                       
                return customer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
