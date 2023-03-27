using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public IAccountService _accountService;

        public CustomerController(ICustomerService customerService, IAccountService accountService)
        {
            this._customerService = customerService;
            this._accountService = accountService;
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

        [HttpPost]
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

        [HttpPut("updateProfileCustomer")]
        public async Task<IActionResult> UpdateProfile([FromBody] CustomerRequestModel input, int customerId)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                Customer existingCustomer = await _customerService.GetById(customerId);
                var accountId = existingCustomer.AccountId;
                int userId = accountId ?? 0;
                Account user = await _accountService.GetById(userId);


                
                if (existingCustomer == null) { return NotFound(); }
                else
                {
                    existingCustomer.Fullname = input.FullName;
                    existingCustomer.Email = input.Email;
                    existingCustomer.UpdatedDate = DateTime.Now;
                    existingCustomer.UpdatedBy = input.FullName;
                    //existingCustomer.Address = input.LocationId;
                    //existingCustomer.
                    await _customerService.Update(existingCustomer);

                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedBy = user.FullName;
                    user.FullName = input.FullName;
                    user.Dob = input.Dob;
                    user.Email = input.Email;
                    user.ProfilePic = input.SavedFileName;
                    
                    await _accountService.Update(user);


                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Updated",
                        Data = existingCustomer
                    });
                }


            }
        }

        [HttpPut("{id}/deactivate")]
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

        [HttpPut("{id}/activate")]
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
