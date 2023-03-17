using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Model.Models;

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
    }
}
