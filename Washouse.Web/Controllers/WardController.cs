using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Infrastructure;

namespace Washouse.Web.Controllers
{
    [Route("api/ward")]
    [ApiController]
    public class WardController : ControllerBase
    {
        #region Initialize
        private IWardService _wardService;
        private ErrorLogger _errorLogger;

        public WardController(IWardService wardService, ErrorLogger errorLogger)
        {
            this._wardService = wardService;
            this._errorLogger = errorLogger;
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
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex);
                return BadRequest();
            }
        }
    }
}
