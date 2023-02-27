//using GoogleMaps.LocationServices;
using GoogleMaps.LocationService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Washouse.Web.Controllers
{
    [Route("api/googleMaps")]
    [ApiController]
    public class GoogleMapsController : ControllerBase
    {
        [HttpGet("getDistrictFromLatLong")]
        public IActionResult GetDistrictFromLatLong(double latitude, double longitude)
        {
            Region result = new Region();
            try
            {
                var gls = new GoogleLocationService("AIzaSyClrZ2yYY2WEOonW9QuK_ir0JXprsEweYM");
                result = gls.GetDistrictFromLatLong(latitude, longitude);
                if (result == null)
                {
                    return NotFound("Can not get district from this latitude and longitude");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { District = result.Name });
        }
    }
}
