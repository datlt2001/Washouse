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
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/maps")]
    [ApiController]
    public class MapsController : ControllerBase
    {
        #region Initialize
        private readonly IDistrictService _districtService;
        private readonly IWardService _wardService;

        public MapsController(IDistrictService districtService, IWardService wardService)
        {
            this._districtService = districtService;
            this._wardService = wardService;
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

        [HttpGet("getLocationFromMap")]
        public async Task<IActionResult> getLocationFromMap(decimal latitude, decimal longitude)
        {
            string url = $"https://nominatim.openstreetmap.org/reverse?email=thanhdat3001@gmail.com&format=jsonv2&lat={latitude}&lon={longitude}";
            District district = new District();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JObject jObject = JObject.Parse(json);
                        string DistrictNameResponse = (string)jObject["address"]["city_district"];
                        string WardNameResponse = (string)jObject["address"]["suburb"];
                        string CityNameResponse = ((string)jObject["address"]["city"] != null) ? (string)jObject["address"]["city"] : "Not found";
                        if (!CityNameResponse.ToLower().Contains("Hồ Chí Minh".ToLower()) && !CityNameResponse.ToLower().Contains("Thủ Đức".ToLower()))
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status404NotFound,
                                Message = "Location not in Ho Chi Minh City",
                                Data = null
                            });
                        }
                        string DistrictName = CityNameResponse;
                        if (DistrictNameResponse != null)
                        {
                            DistrictName = Utilities.MapDistrictName(DistrictNameResponse);
                        }

                        if (DistrictName == null)
                        {
                            DistrictName = DistrictNameResponse;
                        }
                        district = await _districtService.GetDistrictByName(DistrictName);
                        var ward = await _wardService.GetWardByName(WardNameResponse);
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = new 
                            {
                                DistrictId = district.Id,
                                DistrictName = district.DistrictName,
                                WardId = ward.Id,
                                WardName = ward.WardName
                            }
                        });
                    }
                    else
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = response.ReasonPhrase,
                            Data = null
                        });
                    }
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
    }
}
