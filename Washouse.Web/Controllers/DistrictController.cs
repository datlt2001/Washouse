using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Interface;
using Washouse.Web.Infrastructure;
using System.Collections.Generic;
using Washouse.Model.ResponseModels;

namespace Washouse.Web.Controllers
{
    [Route("api/district")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        #region Initialize
        private IDistrictService _districtService;
        private ErrorLogger _errorLogger;

        public DistrictController(IDistrictService districtService, ErrorLogger errorLogger)
        {
            this._districtService = districtService;
            this._errorLogger = errorLogger;
        }

        #endregion

        [Route("getAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var districtList = await _districtService.GetAll();
                var response = new List<DistrictResponseModel>();
                foreach (var district in districtList)
                {
                    response.Add(new DistrictResponseModel
                    {
                        DistrictID = district.Id,
                        DistrictName = district.DistrictName
                    });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex);
                return BadRequest();
            }
        }
    }
}
