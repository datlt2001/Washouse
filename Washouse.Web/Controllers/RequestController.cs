using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using System.Collections.Generic;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Web.Models;
using Washouse.Common.Helpers;
using System.Linq;
using System.Security.Claims;

namespace Washouse.Web.Controllers
{
    [Route("api/requests")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        #region Initialize
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ICenterRequestService _centerRequestService;
        private readonly IServiceService _serviceService;
        private readonly ICenterService _centerService;
        private readonly ICloudStorageService _cloudStorageService;

        public RequestController(IServiceRequestService serviceRequestService, 
            IServiceService serviceService, ICenterRequestService centerRequestService, ICenterService centerService, ICloudStorageService cloudStorageService)
        {
            this._serviceRequestService = serviceRequestService;
            this._serviceService = serviceService;
            this._centerRequestService = centerRequestService;
            this._centerService = centerService;
            this._cloudStorageService = cloudStorageService;
        }
        #endregion

        [HttpGet("centers")]
        public async Task<IActionResult> GetAll([FromQuery] FilterCentersRequestModel filterCentersRequestModel)
        {
            try
            {
                var centerList = await _centerService.GetAll();
                centerList = centerList.Where(center => center.Status == "UpdatePending" || center.Status == "CreatePending");
                if (filterCentersRequestModel.SearchString != null)
                {
                    centerList = centerList.Where(res => res.CenterName.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower())
                                                  || res.Alias.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower())
                                             ).ToList();
                }
                var response = new List<CenterResponseModel>();
                foreach (var center in centerList)
                {
                    var centerServices = new List<CenterServiceResponseModel>();
                    var centerOperatingHours = new List<CenterOperatingHoursResponseModel>();

                    decimal minPrice = 0, maxPrice = 0;
                    foreach (var service in center.Services)
                    {
                        if (service.PriceType)
                        {
                            foreach (var servicePrice in service.ServicePrices)
                            {
                                if (minPrice > service.Price || minPrice == 0)
                                {
                                    minPrice = (decimal)service.Price;
                                }
                            }
                        }
                        else
                        {
                            if (minPrice > service.Price || minPrice == 0)
                            {
                                minPrice = (decimal)service.Price;
                            }
                            if (maxPrice < service.Price || maxPrice == 0)
                            {
                                maxPrice = (decimal)service.Price;
                            }
                        }

                        var centerService = new CenterServiceResponseModel
                        {
                            ServiceCategoryID = service.CategoryId,
                            ServiceCategoryName = service.Category.CategoryName,
                            Services = null
                        };
                        if (centerServices.FirstOrDefault(cs => cs.ServiceCategoryID == centerService.ServiceCategoryID) == null) centerServices.Add(centerService);
                    }
                    List<int> dayOffs = new List<int>();
                    bool MonthOff = false;
                    for (int i = 0; i < 7; i++)
                    {
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
                    double distance = 0;
                    if (center.Location.Latitude == null || center.Location.Longitude == null || filterCentersRequestModel.CurrentUserLatitude == null || filterCentersRequestModel.CurrentUserLongitude == null)
                    {
                        distance = 0;
                    }
                    else
                    {
                        distance = Utilities.CalculateDistance(Math.Round((decimal)filterCentersRequestModel.CurrentUserLatitude, 6), Math.Round((decimal)filterCentersRequestModel.CurrentUserLongitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
                    }
                    if (center.MonthOff != null)
                    {
                        string[] offs = center.MonthOff.Split('-');
                        for (int i = 0; i < offs.Length; i++)
                        {
                            if (DateTime.Now.Day == (int.Parse(offs[i])))
                            {
                                MonthOff = true;
                            }
                        }
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
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        HasDelivery = center.HasDelivery,
                        CenterLocation = new CenterLocationResponseModel
                        {
                            Latitude = center.Location.Latitude,
                            Longitude = center.Location.Longitude
                        },
                        CenterOperatingHours = centerOperatingHours.OrderBy(a => a.Day).ToList(),
                        MonthOff = MonthOff
                    });
                }
                if (filterCentersRequestModel.Sort != null)
                {
                    string[] sorts = filterCentersRequestModel.Sort.Split(',');
                    foreach (var item in sorts)
                    {
                        if (item.Equals("Rating"))
                        {
                            response = response.OrderByDescending(res => res.Rating).ThenBy(res => res.Distance).ToList();
                        }
                        else if (item.Equals("Location"))
                        {
                            response = response.OrderBy(res => res.Distance).ThenByDescending(res => res.Rating).ToList();
                        }
                    }
                }
                if (filterCentersRequestModel.BudgetRange != null)
                {
                    string[] budgetRanges = filterCentersRequestModel.BudgetRange.Split('-');
                    decimal minPrice = decimal.Parse(budgetRanges[0]);
                    decimal maxPrice = decimal.Parse(budgetRanges[1]);
                    if (minPrice > maxPrice)
                    {
                        decimal exchange = minPrice;
                        minPrice = maxPrice;
                        maxPrice = exchange;
                    }
                    foreach (var item in response.ToList())
                    {
                        if (item.MaxPrice < minPrice || item.MinPrice > maxPrice)
                        {
                            response.Remove(item);
                        }
                    }
                }
                if (filterCentersRequestModel.CategoryServices != null)
                {
                    string[] categories = filterCentersRequestModel.CategoryServices.Split(',');
                    var categoryIds = new int[categories.Length];
                    for (int i = 0; i < categories.Length; i++)
                    {
                        categoryIds[i] = int.Parse(categories[i]);
                    }
                    foreach (var item in response.ToList())
                    {
                        int count = 0;
                        foreach (var category in item.CenterServices)
                        {
                            for (int i = 0; i < categoryIds.Length; i++)
                            {
                                if (category.ServiceCategoryID == categoryIds[i])
                                {
                                    count++;
                                }
                            }
                        }
                        if (count == 0)
                        {
                            response.Remove(item);
                        }
                    }

                }
                int totalItems = response.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filterCentersRequestModel.PageSize);

                response = response.Skip((filterCentersRequestModel.Page - 1) * filterCentersRequestModel.PageSize).Take(filterCentersRequestModel.PageSize).ToList();

                if (response.Count != 0)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            ItemsPerPage = filterCentersRequestModel.PageSize,
                            PageNumber = filterCentersRequestModel.Page,
                            Items = response
                        }
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
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


        [HttpGet("centers/{id}")]
        public async Task<IActionResult> GetById(int id)
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
                    var centerDeliveryPrices = new List<CenterDeliveryPriceChartResponseModel>();
                    foreach (var item in center.Services)
                    {
                        var servicePriceViewModels = new List<ServicePriceViewModel>();
                        foreach (var servicePrice in item.ServicePrices)
                        {
                            var sp = new ServicePriceViewModel
                            {
                                MaxValue = servicePrice.MaxValue,
                                Price = servicePrice.Price
                            };
                            servicePriceViewModels.Add(sp);
                        }
                        var service = new ServicesOfCenterResponseModel
                        {
                            ServiceId = item.Id,
                            CategoryId = item.CategoryId,
                            ServiceName = item.ServiceName,
                            Description = item.Description,
                            Image = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
                            PriceType = item.PriceType,
                            Price = item.Price,
                            MinPrice = item.MinPrice,
                            Prices = servicePriceViewModels,
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
                    //}
                    bool MonthOff = false;
                    if (center.MonthOff != null)
                    {
                        string[] offs = center.MonthOff.Split('-');
                        for (int i = 0; i < offs.Length; i++)
                        {
                            if (DateTime.Now.Day == (int.Parse(offs[i])))
                            {
                                MonthOff = true;
                            }
                        }
                    }

                    foreach (var item in center.DeliveryPriceCharts)
                    {
                        var centerDeliveryPrice = new CenterDeliveryPriceChartResponseModel
                        {
                            Id = item.Id,
                            MaxDistance = item.MaxDistance,
                            MaxWeight = item.MaxWeight,
                            Price = item.Price
                        };
                        centerDeliveryPrices.Add(centerDeliveryPrice);
                    }


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
                    //response.Distance = distance;
                    response.MonthOff = MonthOff;
                    response.HasDelivery = center.HasDelivery;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours;
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = response
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
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

        [HttpPut("centers/{id}/approve")]
        public async Task<IActionResult> ApproveCreateCenter(int id)
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
                    var centerDeliveryPrices = new List<CenterDeliveryPriceChartResponseModel>();
                    foreach (var item in center.Services)
                    {
                        var servicePriceViewModels = new List<ServicePriceViewModel>();
                        foreach (var servicePrice in item.ServicePrices)
                        {
                            var sp = new ServicePriceViewModel
                            {
                                MaxValue = servicePrice.MaxValue,
                                Price = servicePrice.Price
                            };
                            servicePriceViewModels.Add(sp);
                        }
                        var service = new ServicesOfCenterResponseModel
                        {
                            ServiceId = item.Id,
                            CategoryId = item.CategoryId,
                            ServiceName = item.ServiceName,
                            Description = item.Description,
                            Image = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
                            PriceType = item.PriceType,
                            Price = item.Price,
                            MinPrice = item.MinPrice,
                            Prices = servicePriceViewModels,
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
                    //}
                    bool MonthOff = false;
                    if (center.MonthOff != null)
                    {
                        string[] offs = center.MonthOff.Split('-');
                        for (int i = 0; i < offs.Length; i++)
                        {
                            if (DateTime.Now.Day == (int.Parse(offs[i])))
                            {
                                MonthOff = true;
                            }
                        }
                    }

                    foreach (var item in center.DeliveryPriceCharts)
                    {
                        var centerDeliveryPrice = new CenterDeliveryPriceChartResponseModel
                        {
                            Id = item.Id,
                            MaxDistance = item.MaxDistance,
                            MaxWeight = item.MaxWeight,
                            Price = item.Price
                        };
                        centerDeliveryPrices.Add(centerDeliveryPrice);
                    }


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
                    //response.Distance = distance;
                    response.MonthOff = MonthOff;
                    response.HasDelivery = center.HasDelivery;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours;
                    center.Status = "Active";
                    await _centerService.Update(center);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = response
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
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

        [HttpPut("centers/{id}/reject")]
        public async Task<IActionResult> RejectCreateCenter(int id)
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
                    var centerDeliveryPrices = new List<CenterDeliveryPriceChartResponseModel>();
                    foreach (var item in center.Services)
                    {
                        var servicePriceViewModels = new List<ServicePriceViewModel>();
                        foreach (var servicePrice in item.ServicePrices)
                        {
                            var sp = new ServicePriceViewModel
                            {
                                MaxValue = servicePrice.MaxValue,
                                Price = servicePrice.Price
                            };
                            servicePriceViewModels.Add(sp);
                        }
                        var service = new ServicesOfCenterResponseModel
                        {
                            ServiceId = item.Id,
                            CategoryId = item.CategoryId,
                            ServiceName = item.ServiceName,
                            Description = item.Description,
                            Image = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
                            PriceType = item.PriceType,
                            Price = item.Price,
                            MinPrice = item.MinPrice,
                            Prices = servicePriceViewModels,
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
                    //}
                    bool MonthOff = false;
                    if (center.MonthOff != null)
                    {
                        string[] offs = center.MonthOff.Split('-');
                        for (int i = 0; i < offs.Length; i++)
                        {
                            if (DateTime.Now.Day == (int.Parse(offs[i])))
                            {
                                MonthOff = true;
                            }
                        }
                    }

                    foreach (var item in center.DeliveryPriceCharts)
                    {
                        var centerDeliveryPrice = new CenterDeliveryPriceChartResponseModel
                        {
                            Id = item.Id,
                            MaxDistance = item.MaxDistance,
                            MaxWeight = item.MaxWeight,
                            Price = item.Price
                        };
                        centerDeliveryPrices.Add(centerDeliveryPrice);
                    }


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
                    //response.Distance = distance;
                    response.MonthOff = MonthOff;
                    response.HasDelivery = center.HasDelivery;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours;
                    center.Status = "Reject";
                    await _centerService.Update(center);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = response
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
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

        /*[HttpPost("updateService")]
        public async Task<IActionResult> Update([FromForm]ServiceRequestModel serviceRequestmodel, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ServiceRequest serviceRequest = new ServiceRequest();
                    var service = await _serviceService.GetById(id);
                    if (service == null)
                    {
                        return NotFound();
                    } else
                    {
                        serviceRequest.Id = 0;
                        serviceRequest.ServiceRequesting = service.Id;
                        serviceRequest.RequestStatus = true;
                        serviceRequest.ServiceName = serviceRequestmodel.ServiceName;
                        serviceRequest.Alias = serviceRequestmodel.Alias;
                        serviceRequest.CategoryId = serviceRequestmodel.CategoryId;
                        serviceRequest.Description = serviceRequestmodel.Description;
                        serviceRequest.PriceType = serviceRequestmodel.PriceType;
                        serviceRequest.Image = serviceRequestmodel.Image;
                        if (!serviceRequest.PriceType)
                        {
                            serviceRequest.Price = serviceRequestmodel.Price;
                        }
                        serviceRequest.TimeEstimate = serviceRequestmodel.TimeEstimate;
                        serviceRequest.Status = service.Status;
                        serviceRequest.HomeFlag = service.HomeFlag;
                        serviceRequest.HotFlag = service.HotFlag;
                        serviceRequest.Rating = service.Rating;
                        serviceRequest.CreatedDate = service.CreatedDate;
                        serviceRequest.UpdatedDate = DateTime.Now;
                        var result = _serviceRequestService.Add(serviceRequest);
                        return Ok(result);
                    }
                }
                else { return BadRequest(); }
            }
            catch
            {
                    return BadRequest();
            }
        }*/

    }
}
