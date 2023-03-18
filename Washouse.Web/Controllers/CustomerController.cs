using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Service.Implement;
using Washouse.Service.Interface;

namespace Washouse.Web.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }

        [HttpGet]
        public IActionResult GetCustomerList()
        {
            var customer = _customerService.GetAll();
            if (customer == null) { return NotFound(); }
            return Ok(customer);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetById(id);
            if (customer == null) { return NotFound(); }
            return Ok(customer);
        }

        [HttpPost("addCustomer")]
        public IActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.Id = 0;
                customer.CreatedDate = DateTime.Now;
                customer.UpdatedDate = DateTime.Now;
                customer.Status = false;
                var customers = _customerService.Add(customer);
                return Ok(customers);
            }
            else { return BadRequest(); }

        }

        [HttpPut("updateCustomer")]
        public async Task<IActionResult> Update(Customer customer)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                //Category existingCategorySevice =  await _serviceCategoryService.GetById(id);
                Customer existingCustomer = new Customer();
                if (existingCustomer == null) { return NotFound(); }
                else
                {
                    customer.Id = existingCustomer.Id;
                    existingCustomer = customer;

                    await _customerService.Update(existingCustomer);
                    return Ok(existingCustomer);
                }


            }
        }

        [HttpPut("deactivateCustomer/{id}")]
        public async Task<IActionResult> DeactivateCustomer(int id)
        {
            var customer = await _customerService.GetById(id);
            if (customer == null)
            {
                return NotFound();
            }
            await _customerService.DeactivateCustomer(id);
            return Ok();
        }

        [HttpPut("activateCustomer/{id}")]
        public async Task<IActionResult> ActivateCustomer(int id)
        {
            var customer = await _customerService.GetById(id);
            if (customer == null)
            {
                return NotFound();
            }
            await   _customerService.ActivateCustomer(id);
            return Ok();
        }

    }
}
