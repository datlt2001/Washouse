using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/center")]
    [ApiController]
    public class CenterController : ControllerBase
    {
        #region Initialize
        private readonly ICenterService _centerService; 
        private readonly ICloudStorageService _cloudStorageService;
        public CenterController(ICenterService centerService, ICloudStorageService cloudStorageService)
        {
            this._centerService = centerService; 
            _cloudStorageService = cloudStorageService;
        }

        #endregion

        [Route("getAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll(decimal? UserLatitude, decimal? UserLongitude)
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
                            ServiceCategoryName = service.Category.CategoryName,
                            Services = null
                        };
                        if (centerServices.FirstOrDefault(cs => cs.ServiceCategoryID == centerService.ServiceCategoryID) == null) centerServices.Add(centerService);
                    }
                    //int nowDayOfWeek = ((int)DateTime.Today.DayOfWeek != 0) ? (int)DateTime.Today.DayOfWeek : 8;
                    //if (center.OperatingHours.FirstOrDefault(a => a.DaysOfWeekId == nowDayOfWeek) != null)
                    //{
                    List<int> dayOffs = new List<int>();
                    for (int i = 0; i < 7; i++) {
                        dayOffs.Add(i);
                    }
                    foreach (var item in center.OperatingHours)
                    {
                        dayOffs.Remove(item.DaysOfWeek.Id);
                        var centerOperatingHour = new CenterOperatingHoursResponseModel
                        {
                            Day = item.DaysOfWeek.Id,
                            OpenTime = item.OpenTime,
                            CloseTime = item.CloseTime
                        };
                        centerOperatingHours.Add(centerOperatingHour);
                    }
                    foreach (var item in dayOffs)
                    {
                        var dayOff = new CenterOperatingHoursResponseModel
                        {
                            Day = item,
                            OpenTime = null,
                            CloseTime = null
                        };
                        centerOperatingHours.Add(dayOff);
                    }
                    //}

                    double distance = 0;
                    if (center.Location.Latitude == null || center.Location.Longitude == null || UserLatitude == null || UserLongitude == null)
                    {
                        distance = 0;
                    } else
                    {
                        distance = Utilities.CalculateDistance(Math.Round((decimal)UserLatitude, 6), Math.Round((decimal)UserLongitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
                    }
                    response.Add(new CenterResponseModel
                    {
                        Id = center.Id,
                        Thumbnail = center.Image != null ? await _cloudStorageService.GetSignedUrlAsync(center.Image) : null,
                        Title = center.CenterName,
                        Alias = center.Alias,
                        Description = center.Description,
                        CenterServices = centerServices,
                        Rating = center.Rating,
                        NumOfRating = center.NumOfRating,
                        Phone = center.Phone,
                        CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName + ", " + center.Location.Ward.District.DistrictName,
                        Distance = Math.Round(distance, 1),
                        CenterLocation = new CenterLocationResponseModel
                        {
                            Latitude = center.Location.Latitude,
                            Longitude = center.Location.Longitude
                        },
                        CenterOperatingHours = centerOperatingHours.OrderBy(a => a.Day).ToList(),
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
        public async Task<IActionResult> GetById(int id, decimal? UserLatitude, decimal? UserLongitude)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    var response = new CenterResponseModel();
                    var centerServices = new List<CenterServiceResponseModel>();
                    var centerOperatingHours = new List<CenterOperatingHoursResponseModel>();
                    var servicesOfCenter = new List<ServicesOfCenterResponseModel>();
                    foreach (var item in center.Services)
                    {
                        var service = new ServicesOfCenterResponseModel
                        {
                            ServiceId = item.Id,
                            CategoryId = item.CategoryId,
                            ServiceName = item.ServiceName,
                            Description = item.Description,
                            Image  = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
                            Price = item.Price == null ? 0 : (decimal)item.Price,
                            TimeEstimate = item.TimeEstimate,
                            Rating = item.Rating,
                            NumOfRating = item.NumOfRating
                        };
                        servicesOfCenter.Add(service);
                    }
                    foreach (var service in center.Services)
                    {
                        var centerService = new CenterServiceResponseModel
                        {
                            ServiceCategoryID = service.CategoryId,
                            ServiceCategoryName = service.Category.CategoryName,
                            Services = servicesOfCenter.Where(ser => ser.CategoryId == service.CategoryId).ToList()
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
                    double distance = 0;
                    if (center.Location.Latitude == null || center.Location.Longitude == null || UserLatitude == null || UserLongitude == null)
                    {
                        distance = 0;
                    }
                    else
                    {
                        distance = Utilities.CalculateDistance(Math.Round((decimal)UserLatitude, 6), Math.Round((decimal)UserLongitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
                    }
                    //}
                    response.Id = center.Id;
                    response.Thumbnail = center.Image != null ? await _cloudStorageService.GetSignedUrlAsync(center.Image) : null;
                    response.Title = center.CenterName;
                    response.Alias = center.Alias;
                    response.Description = center.Description;
                    response.CenterServices = centerServices;
                    response.Rating = center.Rating;
                    response.NumOfRating = center.NumOfRating;
                    response.Phone = center.Phone;
                    response.CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName + ", " + center.Location.Ward.District.DistrictName;
                    response.Distance = distance;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours;
                    return Ok(response);
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
            catch
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
            catch
            {
                return BadRequest();
            }
        }
    }
}
