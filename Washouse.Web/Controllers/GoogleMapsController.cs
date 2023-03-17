//using GoogleMaps.LocationServices;
using GoogleMaps.LocationService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Web.Controllers
{
    [Route("api/googleMaps")]
    [ApiController]
    public class GoogleMapsController : ControllerBase
    {
        #region Initialize
        private readonly IDistrictService _districtService;

        public GoogleMapsController(IDistrictService districtService)
        {
            this._districtService = districtService;
        }

        #endregion
        [HttpGet("getDistrictFromLatLong")]
        public async Task<IActionResult> GetDistrictFromLatLong(double latitude, double longitude)
        {
            Region result = new Region();
            District district = new District();
            try
            {
                var gls = new GoogleLocationService("AIzaSyClrZ2yYY2WEOonW9QuK_ir0JXprsEweYM");
                result = gls.GetDistrictFromLatLong(latitude, longitude);
                if (result == null)
                {
                    return NotFound("Can not get district from this latitude and longitude");
                }
                string DistrictName = Utilities.MapDistrictName(result.Name);
                if (DistrictName == null)
                {
                    DistrictName = result.Name;
                }
                district = await _districtService.GetDistrictByName(DistrictName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { DistrictId = district.Id, 
                            DistricName = district.DistrictName
            });
        }
    }
}
