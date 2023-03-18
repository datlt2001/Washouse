using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;

namespace Washouse.Web.Controllers
{
    [Route("api/ward")]
    [ApiController]
    public class WardController : ControllerBase
    {
        #region Initialize
        private readonly IWardService _wardService;

        public WardController(IWardService wardService)
        {
            this._wardService = wardService;
        }
        #endregion

        [HttpGet("getWardListByDistrict/{id}")]
        public async Task<IActionResult> GetWardListByDistrict(int id)
        {
            try
            {
                var wardList = await _wardService.GetWardListByDistrictId(id);
                if (wardList != null)
                {
                    return Ok(wardList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch {
                return BadRequest();
            }
        }
    }
}
