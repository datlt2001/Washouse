using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
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
        public IWardService _wardService;
        public ILocationService _locationService;
        public ICloudStorageService _cloudStorageService;
        public IDistrictService _districtService;

        public CustomerController(ICustomerService customerService, IAccountService accountService, IWardService wardService,
            ILocationService locationService, ICloudStorageService cloudStorageService, IDistrictService districtService)
        {
            this._customerService = customerService;
            this._accountService = accountService;
            this._wardService = wardService;
            _locationService = locationService;
            _cloudStorageService = cloudStorageService;
            _districtService = districtService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetCustomerList()
        {
            var customer = _customerService.GetAll();
            if (customer == null) { return NotFound(); }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = customer
            });
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomerById(int customerId)
        {
            var customer = await _customerService.GetById(customerId);
            int userId = customer.AccountId ?? 0;
            var user = await _accountService.GetById(userId);
            var response = new CustomerDetailResponseModel();
            if(customer.Address != null) 
            {
                int locationid = customer.Address ?? 0;
                var location = await _locationService.GetById(locationid);
                var ward = await _wardService.GetWardById(location.WardId);
                var district = await _districtService.GetDistrictById(ward.DistrictId);
                response.AddressString = location.AddressString +", "+ ward.WardName + ", " + district.DistrictName + ", " + "Thành Phố Hồ Chí Minh";
                response.Address = new CustomerLocatonResponseModel
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    AddressString = location.AddressString,
                    Ward = new WardResponseModel
                    {
                        WardId = location.WardId,
                        WardName = ward.WardName,
                        District = new DistrictResponseModel
                        {
                            DistrictId = ward.DistrictId,
                            DistrictName = district.DistrictName
                        }
                    },
                    

                };
            }
            else
            {
                response.AddressString = null;
            }

            
            response.Fullname = customer.Fullname;
            response.Phone = customer.Phone;
            response.Email = customer.Email;
            response.Gender = user.Gender;
            response.WalletId = user.WalletId;
            if (user.Dob != null)
            {
                string dob = user.Dob.Value.ToString("dd-MM-yyyy HH-mm-ss");
                response.Dob = dob;
            }
            else
            {
                response.Dob = null;
            }
            if (user != null)
            {
                response.ProfilePic = user.ProfilePic != null ? await _cloudStorageService.GetSignedUrlAsync(user.ProfilePic) : null;
            }
            response.AccountId = userId;
            response.Id = customerId;
            
            if (customer == null) { return NotFound(); }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = response
            });
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

        [HttpPut("profilepic")]
        public async Task<IActionResult> UpdateProfilePic(string? SavedFileName)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                int customerId = int.Parse(User.FindFirst("Id")?.Value);
                Customer existingCustomer = await _customerService.GetById(customerId);
                if (existingCustomer == null) { return NotFound(); }
                else
                {
                    var userid = existingCustomer.AccountId ?? 0;
                    Account account = await _accountService.GetById(userid);
                    account.ProfilePic = SavedFileName;

                    await _accountService.Update(account);
                    return Ok(account);
                }


            }
        }

        /*[Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfileInfo([FromBody] CustomerRequestModel input)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                int customerId = int.Parse(User.FindFirst("Id")?.Value);
                Customer existingCustomer = await _customerService.GetById(customerId);
                var accountId = existingCustomer.AccountId;
                int userId = accountId ?? 0;
                Account user = await _accountService.GetById(userId);


                
                if (existingCustomer == null) { return NotFound(); }
                else
                {
                    existingCustomer.Fullname = input.FullName;
                    //existingCustomer.Email = input.Email;
                    existingCustomer.UpdatedDate = DateTime.Now;
                    existingCustomer.UpdatedBy = input.FullName;
                    //existingCustomer.Address = input.LocationId;
                    //existingCustomer.
                    await _customerService.Update(existingCustomer);

                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedBy = user.FullName;
                    user.FullName = input.FullName;
                    int age = DateTime.Now.Subtract((DateTime)input.Dob).Days;
                    if(age > 18 && age < 80)
                    {
                        user.Dob = input.Dob;
                    }
                    else
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Not Updated because your age is not suitable for use this platform",
                            Data = null
                        });
                    }
                    
                    //user.Email = input.Email;
                    //user.ProfilePic = input.SavedFileName;
                    
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
*/
        
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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



        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetCustomerByAccountId(int accountId)
        {
            var user = await _accountService.GetById(accountId);
            var customer = await _customerService.GetCustomerByAccID(user.Id);
            var response = new CustomerDetailResponseModel();
            if (customer.Address != null)
            {
                int locationid = customer.Address ?? 0;
                var location = await _locationService.GetById(locationid);
                var ward = await _wardService.GetWardById(location.WardId);
                var district = await _districtService.GetDistrictById(ward.DistrictId);
                response.AddressString = location.AddressString + ", " + ward.WardName + ", " + district.DistrictName + ", " + "Thành Phố Hồ Chí Minh";
                response.Address = new CustomerLocatonResponseModel
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    AddressString = location.AddressString,
                    Ward = new WardResponseModel
                    {
                        WardId = location.WardId,
                        WardName = ward.WardName,
                        District = new DistrictResponseModel
                        {
                            DistrictId = ward.DistrictId,
                            DistrictName = district.DistrictName
                        }
                    },
                };
            }
            else
            {
                response.AddressString = null;
            }


            response.Fullname = customer.Fullname;
            response.Phone = customer.Phone;
            response.Email = customer.Email;
            response.Gender = user.Gender;
            response.WalletId = user.WalletId;
            if (user.Dob != null)
            {
                string dob = user.Dob.Value.ToString("dd-MM-yyyy HH-mm-ss");
                response.Dob = dob;
            }
            else
            {
                response.Dob = null;
            }
            if (user != null)
            {
                response.ProfilePic = user.ProfilePic != null ? await _cloudStorageService.GetSignedUrlAsync(user.ProfilePic) : null;
            }
            response.AccountId = accountId;
            response.Id = customer.Id;

            if (customer == null) { return NotFound(); }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = response
            });

        }

    }
}
