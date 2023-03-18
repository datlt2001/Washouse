using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Washouse.Service.Implement;
using Washouse.Service.Interface;

namespace Washouse.Web.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        public readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            this._staffService = staffService;
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

        [HttpPut("deactivateStaff/{id}")]
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

        [HttpPut("activateStaff/{id}")]
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
    }
}
