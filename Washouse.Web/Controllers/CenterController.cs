using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Infrastructure;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/center")]
    [ApiController]
    public class CenterController : ControllerBase
    {
        #region Initialize
        private ICenterService _centerService;
        public CenterController(ICenterService centerService)
        {
            this._centerService = centerService;
        }

        #endregion

        [Route("getAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var centerList = await _centerService.GetAll();
                var response = new List<CenterResponseModel>();
                var centerServices = new List<CenterServiceResponseModel>();
                var centerOperatingHours = new List<CenterOperatingHoursResponseModel>();
                foreach (var center in centerList)
                {
                    foreach (var service in center.Services)
                    {
                        var centerService = new CenterServiceResponseModel
                        {
                            ServiceCategoryID = service.CategoryId,
                            ServiceCategoryName = service.Category.CategoryName
                        };
                        if (centerServices.FirstOrDefault(cs => cs.ServiceCategoryID == centerService.ServiceCategoryID) == null) centerServices.Add(centerService);
                    }
                    //int nowDayOfWeek = ((int)DateTime.Today.DayOfWeek != 0) ? (int)DateTime.Today.DayOfWeek : 8;
                    //if (center.OperatingHours.FirstOrDefault(a => a.DaysOfWeekId == nowDayOfWeek) != null)
                    //{
                        foreach (var item in center.OperatingHours)
                        {
                            var centerOperatingHour = new CenterOperatingHoursResponseModel
                            {
                                Day = item.DaysOfWeek.Id,
                                OpenTime = item.OpenTime,
                                CloseTime = item.CloseTime
                            };
                            centerOperatingHours.Add(centerOperatingHour);
                        }
                    //}
                    response.Add(new CenterResponseModel
                    {
                        Id = center.Id,
                        Thumbnail = center.Image,
                        Title = center.CenterName,
                        Alias = center.Alias,
                        Description = center.Description,
                        CenterServices = centerServices,
                        Rating = center.Rating,
                        NumOfRating = center.NumOfRating,
                        Phone = center.Phone,
                        CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName + ", " + center.Location.Ward.District.DistrictName,
                        CenterLocation = new CenterLocationResponseModel
                        {
                            Latitude = center.Location.Latitude,
                            Longitude = center.Location.Longitude
                        },
                        CenterOperatingHours = centerOperatingHours
                    });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        //[Route("Details/{id}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    return Ok(center);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string searchKey, int page, int pageSize)
        {
            int totalRow = 0;
            try
            {
                var centerList = _centerService.GetAllBySearchKeyPaging(searchKey, page, pageSize, out totalRow);
                return Ok(centerList);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("createCenter")]
        public async Task<IActionResult> CreateCenter([FromForm] CenterRequestModel centerRequestModel)
        {
            try
            {
                Center center = new Center();
                if (ModelState.IsValid)
                {
                    center.Id = 0;
                    center.CenterName = centerRequestModel.CenterName;
                    center.CenterName = centerRequestModel.CenterName;
                    center.CenterName = centerRequestModel.CenterName;
                    center.CenterName = centerRequestModel.CenterName;
                    /*if (!serviceRequest.PriceType)
                    {
                        serviceRequest.Price = serviceRequestmodel.Price;
                    }
                    serviceRequest.TimeEstimate = serviceRequestmodel.TimeEstimate;
                    serviceRequest.Status = "addRequest";
                    serviceRequest.HomeFlag = false;
                    serviceRequest.HotFlag = false;
                    serviceRequest.Rating = 0;
                    serviceRequest.CreatedDate = DateTime.Now;
                    serviceRequest.UpdatedDate = DateTime.Now;
                    serviceRequest.CenterId =*/
                    var result = _centerService.Add(center);
                    return Ok(result);
                }
                else { return BadRequest(); }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
