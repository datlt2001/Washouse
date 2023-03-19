using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Model.Models;
using Washouse.Model.ViewModel;
using Washouse.Model.ResponseModels;
using Washouse.Web.Models;
using static Google.Apis.Requests.BatchRequest;

namespace Washouse.Web.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        #region Initialize
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            this._locationService = locationService;
        }

        #endregion

        [HttpPost("createLocation")]
        public async Task<IActionResult> CreateLocation([FromForm] LocationRequestModel locationRequest)
        {
            try
            {
                Location location = new Location();
                if (ModelState.IsValid)
                {
                    location.AddressString = locationRequest.AddressString;
                    location.WardId = locationRequest.WardId;
                    var result = _locationService.Add(location);
                    return RedirectToAction(nameof(GetById), new {locationId = location.Id});
                    //return Ok(result);
                }
                else { return BadRequest(); }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("{locationId}")]
        public async Task<IActionResult> GetById(int locationId)
        {
            var location = await _locationService.GetById(locationId);
            if (location == null)
                return BadRequest("Cannot find location");
            return Ok(location);
        }

        [HttpGet("distance")]
        public IActionResult Distance(decimal Latitude_1, decimal Longitude_1, decimal Latitude_2, decimal Longitude_2)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var location1 = new LocationLatLongViewModel
                    {
                        Latitude = Latitude_1,
                        Longitude = Longitude_1
                    };
                    var location2 = new LocationLatLongViewModel
                    {
                        Latitude = Latitude_2,
                        Longitude = Longitude_2
                    };

                    var result = CalculateDistance(location1, location2);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            Distance = result
                        }
                    });
                }
                else {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Fail to calculator distance",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
        public static double CalculateDistance(LocationLatLongViewModel location1, LocationLatLongViewModel location2)
        {
            const double earthRadius = 6371; // Earth's radius in kilometers

            var lat1 = ToRadians((double)location1.Latitude);
            var lon1 = ToRadians((double)location1.Longitude);
            var lat2 = ToRadians((double)location2.Latitude);
            var lon2 = ToRadians((double)location2.Longitude);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = earthRadius * c;

            return distance;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
