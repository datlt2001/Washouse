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
    public class CustomerService : ICustomerService
    {
        private ICustomerRepository _customerRepository;
        public IUnitOfWork _unitOfWork;

        public CustomerService(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ActivateCustomer(int id)
        {
            await _customerRepository.ActivateCustomer(id);
        }

        public async Task Add(Customer customer)
        {
            await _customerRepository.Add(customer);
        }

        public async Task DeactivateCustomer(int id)
        {
            await _customerRepository.DeactivateCustomer(id);   
        }

        public IEnumerable<Customer> GetAll()
        {
            return _customerRepository.Get();
        }

        public async Task<Customer> GetById(int id)
        {
            return await _customerRepository.GetById(id);
        }

        public async Task<Customer> GetByPhone(string phone)
        {
            var customer = await _customerRepository.GetByPhone(phone);
            return customer;
        }

        public async Task Update(Customer customer)
        {
            await _customerRepository.Update(customer);
        }

        public async Task<IEnumerable<Customer>> CustomersOfCenter(int centerId)
        {
            return await _customerRepository.CustomersOfCenter(centerId);
        }

        public async Task<Customer> GetCustomerByAccID(int accountId)
        {
            return await _customerRepository.GetCustomerByAccID(accountId);
        }

        public async Task<Customer> GetCustomerByAccIDLightWeight(int accountId)
        {
            return await _customerRepository.GetCustomerByAccIDLightWeight(accountId);
        }
    }
}
