using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Interface;
using System.Collections.Generic;
using Washouse.Model.ResponseModels;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Washouse.Model.Models;
using Washouse.Common.Helpers;

namespace Washouse.Web.Controllers
{
    [Route("api/district")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        #region Initialize
        private readonly IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            this._districtService = districtService;
        }

        #endregion

        [Route("getAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var districtList = await _districtService.GetAll();
                districtList = districtList.OrderByDescending(dt => dt.DistrictName);
                var response = new List<DistrictResponseModel>();
                foreach (var district in districtList)
                {
                    response.Add(new DistrictResponseModel
                    {
                        DistrictId = district.Id,
                        DistrictName = district.DistrictName
                    });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("getDistrictByLatLong")]
        public async Task<IActionResult> GetDistrictByLatLong(double latitude, double longitude)
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
                        string DistrictName = Utilities.MapDistrictName(DistrictNameResponse);
                        if (DistrictName == null)
                        {
                            DistrictName = DistrictNameResponse;
                        }
                        district = await _districtService.GetDistrictByName(DistrictName);
                        return Ok(new DistrictResponseModel
                        {
                            DistrictId = district.Id,
                            DistrictName = district.DistrictName
                        });
                    }
                    else
                    {
                        return BadRequest("Error: " + response.ReasonPhrase);
                    }
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
