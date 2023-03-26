using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using static Google.Apis.Requests.BatchRequest;
using Washouse.Web.Models;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels;
using System.Collections.Generic;

namespace Washouse.Web.Controllers
{
    [Route("api/wards")]
    [ApiController]
    public class WardController : ControllerBase
    {
        #region Initialize
        private readonly IWardService _wardService;

        public WardController(IWardService wardService)
        {
            this._wardService = wardService;
        }
        #endregion

        [HttpGet("getWardListByDistrict/{id}")]
        public async Task<IActionResult> GetWardListByDistrict(int id)
        {
            try
            {
                var wardList = await _wardService.GetWardListByDistrictId(id);
                var responseData = new List<WardResponseModel>();
                if (wardList != null)
                {
                    foreach (var item in wardList)
                    {
                        responseData.Add(new WardResponseModel { 
                            WardId = item.Id,
                            WardName = item.WardName
                        });
                    }
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = responseData
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex) {
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
