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
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Net.Http;

namespace Washouse.Web.Controllers
{
    [Route("api/locations")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        #region Initialize
        private readonly ILocationService _locationService;
        private readonly IWardService _wardService;

        public LocationController(ILocationService locationService, IWardService wardService)
        {
            this._locationService = locationService;
            this._wardService = wardService;
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromForm] LocationRequestModel locationRequest)
        {
            try
            {
                Model.Models.Location location = new Model.Models.Location();
                if (ModelState.IsValid)
                {
                    location.AddressString = locationRequest.AddressString;
                    location.WardId = locationRequest.WardId;
                    location.Latitude = locationRequest.Latitude;
                    location.Longitude = locationRequest.Longitude;
                    var result = await _locationService.Add(location);
                    return Ok(result);
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

        /// <summary>
        /// Gets lat/long by address.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/locations/search
        ///     {        
        ///       "addressString": 30 Thủy Lợi,
        ///       "wardId": 41
        ///     }
        /// </remarks>
        /// <returns>Latitude and longitude of this address.</returns>
        /// <response code="200">Success return latitude and longitude of this address</response>   
        /// <response code="404">Not found latitude and longitude of this address</response>   
        /// <response code="400">One or more error occurs</response>   
        // GET: api/centers
        [HttpGet("search")]
        public async Task<IActionResult> search(string AddressString, int WardId)
        {
            try
            { 
                if (ModelState.IsValid)
                {
                    decimal? Latitude = null;
                    decimal? Longitude = null;
                    var ward = await _wardService.GetWardById(WardId);
                    string fullAddress = AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                    string wardAddress = ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                    var result = await SearchRelativeAddress(fullAddress);
                    if (result != null)
                    {

                        Latitude = result.lat;
                        Longitude = result.lon;

                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = new
                            {
                                Latitude = Latitude,
                                Longitude = Longitude
                            }
                        });
                    }
                    else
                    {
                        result = await SearchRelativeAddress(wardAddress);
                        if (result != null)
                        {

                            Latitude = result.lat;
                            Longitude = result.lon;

                            return Ok(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "return ward latitude and longitude",
                                Data = new
                                {
                                    Latitude = Latitude,
                                    Longitude = Longitude
                                }
                            });
                        }
                        else
                        {
                            return NotFound(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status404NotFound,
                                Message = "Not found latitude and longitude of this address",
                                Data = null
                            });
                        }
                    }
                }
                else
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Model is not valid",
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

        private static async Task<dynamic> SearchRelativeAddress(string query)
        {
            string url = $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={query}&format=json&limit=1";
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(json);
                        if (result.Count > 0)
                        {
                            return new
                            {
                                lat = result[0].lat,
                                lon = result[0].lon
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            } catch
            {
                return null;
            }
        }
    }
}
