using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Interface;
using Washouse.Web.Infrastructure;
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
                districtList = districtList.OrderByDescending(dt => dt.DistrictName);
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

        [HttpGet("getDistrictByLatLong")]
        public async Task<IActionResult> getDistrictByLatLong(double latitude, double longitude)
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
                        string DistrictNameResponse = (string)jObject["address"]["city"];
                        string DistrictName = Utilities.MapDistrictName(DistrictNameResponse);
                        if (DistrictName == null)
                        {
                            DistrictName = DistrictNameResponse;
                        }
                        district = await _districtService.GetDistrictByName(DistrictName);
                        return Ok(new
                        {
                            DistrictId = district.Id,
                            DistricName = district.DistrictName
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
