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
    [Route("api/staffs")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        public readonly IStaffService _staffService;
        public IAccountService _accountService;

        public StaffController(IStaffService staffService, IAccountService accountService)
        {
            this._staffService = staffService;
            accountService = _accountService;
        }

        [HttpGet]
        public IActionResult GetStaffList()
        {
            var staff = _staffService.GetAll();
            if (staff == null) { return NotFound(); }
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffrById(int id)
        {
            var staff = await _staffService.GetById(id);
            if (staff == null) { return NotFound(); }
            return Ok(staff);
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateStaff(int id)
        {
            var staff = await _staffService.GetById(id);
            if (staff == null)
            {
                return NotFound();
            }
            await   _staffService.DeactivateStaff(id);
            return Ok();
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateStaff(int id)
        {
            var staff = await _staffService.GetById(id);
            if (staff == null)
            {
                return NotFound();
            }
            await _staffService.ActivateStaff(id);
            return Ok();
        }

        [HttpPut("{staffId}")]
        public async Task<IActionResult> UpdateProfile([FromBody] StaffRequestModel input, int staffId)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                Staff existingStaff = await _staffService.GetById(staffId);
                var accountId = existingStaff.AccountId;
                //int userId = accountId ?? 0;
                Account user = await _accountService.GetById(accountId);

                if (existingStaff == null) { return NotFound(); }
                else
                {
                                     
                    existingStaff.UpdatedDate = DateTime.Now;
                    existingStaff.UpdatedBy = input.FullName;
                    //existingCustomer.Address = input.LocationId;
                    existingStaff.IdNumber = input.IdNumber;
                    //existingStaff.IdFrontImg = input.IdNumber;
                    //existingStaff.IdBackImg = input.IdNumber;

                    await _staffService.Update(existingStaff);

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
                        Data = existingStaff
                    });
                }


            }

            
        }

        
    }
}
