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
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Microsoft.AspNetCore.Authorization;
using Washouse.Model.ResponseModels.AdminResponseModel;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;
using Washouse.Common.Mails;

namespace Washouse.Web.Controllers
{
    [Route("api/requests")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        #region Initialize
        
        private readonly ICenterRequestService _centerRequestService;
        private readonly IServiceService _serviceService;
        private readonly ICenterService _centerService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ILocationService _locationService;
        private readonly IStaffService _staffService;
        private readonly IAccountService _accountService;
        private ISendMailService _sendMailService;

        public RequestController( 
            IServiceService serviceService, ICenterRequestService centerRequestService, ICenterService centerService
            , ICloudStorageService cloudStorageService, IFeedbackService feedbackService,
             ILocationService locationService, IStaffService staffService, IAccountService accountService, ISendMailService sendMailService)
        {
            
            this._serviceService = serviceService;
            this._centerRequestService = centerRequestService;
            this._centerService = centerService;
            this._cloudStorageService = cloudStorageService;
            this._feedbackService = feedbackService;
            this._locationService = locationService;
            this._staffService = staffService;
            this._accountService = accountService;
            this._sendMailService = sendMailService;
        }
        #endregion

        //[Authorize(Roles = "Admin")]
        [HttpGet("centers/updating")]
        public async Task<IActionResult> GetUpdatingCenterRequest([FromQuery] FilterRequestingCenterRequestModel filterCentersRequestModel)
        {
            try
            {
                var updatingCenterList = await _centerRequestService.GetCenterRequests();
                if (filterCentersRequestModel.Status != null)
                {
                    updatingCenterList = updatingCenterList.Where(updatingCenter => updatingCenter.RequestStatus == filterCentersRequestModel.Status).ToList();
                }
                if (updatingCenterList == null || updatingCenterList.Count() < 1) {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found any updating center request",
                        Data = ""
                    });
                }
                
                var response = new List<AdminUpdatingCenterRequestResponseModel>();
                foreach (var center in updatingCenterList)
                {
                    var staffs = _staffService.GetAllByCenterId(center.CenterRequesting);
                    var manager = staffs.FirstOrDefault(st => (st.IsManager != null && (bool)st.IsManager == true));
                    var account = new Account();
                    if (manager != null)
                    {
                        account = await _accountService.GetById(manager.AccountId);
                    }
                    var location = await _locationService.GetById(center.LocationId);
                    response.Add(new AdminUpdatingCenterRequestResponseModel
                    {
                        Id = center.Id,
                        Thumbnail = center.Image != null
                            ? await _cloudStorageService.GetSignedUrlAsync(center.Image)
                            : null,
                        Title = center.CenterName,
                        Alias = center.Alias,
                        Rating = center.Rating,
                        NumOfRating = center.NumOfRating,
                        Phone = center.Phone,
                        Status = center.Status,
                        TaxCode = center.TaxCode.Trim(),
                        ManagerId = manager != null ? manager.Id : null,
                        ManagerName = manager != null ? account.FullName : null,
                        CenterAddress = location.AddressString + ", " + location.Ward.WardName + ", " +
                                        location.Ward.District.DistrictName,
                        RequestStatus = center.RequestStatus
                    });
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

        [Authorize(Roles = "Admin")]
        [HttpGet("centers/updating/{id}")]
        public async Task<IActionResult> GetUpdatingCenterRequestDetail(int id)
        {
            try
            {
                var centerUpdating = await _centerRequestService.GetById(id);
                if (centerUpdating == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }

                if (centerUpdating != null)
                {
                    var center = await _centerService.GetMyCenter(centerUpdating.CenterRequesting);
                    var response = new CenterManagerResponseModel();
                    var centerOperatingHours = new List<CenterOperatingHoursResponseModel>();
                    var centerDeliveryPrices = new List<CenterDeliveryPriceChartResponseModel>();
                    var centerAdditionServices = new List<AdditionServiceCenterModel>();
                    var centerGalleries = new List<CenterGalleryModel>();
                    var centerFeedbacks = new List<FeedbackCenterModel>();
                    var centerResourses = new List<ResourseCenterModel>();
                    
                    List<int> dayOffs = new List<int>();
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

                    bool MonthOff = false;
                    if (centerUpdating.MonthOff != null)
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
                    response.Thumbnail = centerUpdating.Image != null
                        ? await _cloudStorageService.GetSignedUrlAsync(center.Image)
                        : null;
                    response.Title = centerUpdating.CenterName;
                    response.Alias = centerUpdating.Alias;
                    response.Description = centerUpdating.Description;
                    response.Rating = center.Rating;
                    response.NumOfRating = center.NumOfRating;
                    response.Phone = centerUpdating.Phone;
                    var locationU = await _locationService.GetByIdIncludeWardDistrict(centerUpdating.LocationId);
                    response.CenterAddress = locationU.AddressString + ", " + locationU.Ward.WardName +
                                             ", " + locationU.Ward.District.DistrictName;
                    //response.Distance = distance;
                    response.IsAvailable = center.IsAvailable;
                    response.Status = center.Status;
                    response.TaxCode = centerUpdating.TaxCode.Trim();
                    response.TaxRegistrationImage = center.TaxRegistrationImage;
                    response.MonthOff = MonthOff;
                    response.HasDelivery = center.HasDelivery;
                    response.HasOnlinePayment = center.HasOnlinePayment;
                    response.LocationId = centerUpdating.LocationId;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
                    response.LastDeactivate = center.LastDeactivate.HasValue
                            ? (center.LastDeactivate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                            : null;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours.OrderBy(a => a.Day).ToList();
                    foreach (var item in center.AdditionServices)
                    {
                        var additionService = new AdditionServiceCenterModel
                        {
                            AdditionName = item.AdditionName,
                            Alias = item.Alias,
                            Description = item.Description,
                            Image = item.Image,
                            Status = item.Status
                        };
                        centerAdditionServices.Add(additionService);
                    }

                    response.AdditionServices = centerAdditionServices;
                    foreach (var item in center.CenterGalleries)
                    {
                        var centerGallery = new CenterGalleryModel
                        {
                            Image = item.Image,
                            CreatedDate = item.CreatedDate
                        };
                        centerGalleries.Add(centerGallery);
                    }

                    response.CenterGalleries = centerGalleries;
                    //feedback
                    /*foreach (var item in center.Feedbacks)
                    {
                        var centerFeedback = new FeedbackCenterModel
                        {
                            Content = item.Content,
                            Rating = item.Rating,
                            //OrderId = item.OrderId,
                            CreatedBy = item.CreatedBy,
                            CreatedDate = item.CreatedDate,
                            ReplyMessage = item.ReplyMessage,
                            ReplyBy = item.ReplyBy,
                            ReplyDate = item.ReplyDate
                        };
                        centerFeedbacks.Add(centerFeedback);
                    }

                    response.CenterFeedbacks = centerFeedbacks;*/
                    //resource
                    foreach (var item in center.Resourses)
                    {
                        var centerResourse = new ResourseCenterModel
                        {
                            ResourceName = item.ResourceName,
                            Alias = item.Alias,
                            Quantity = item.Quantity,
                            AvailableQuantity = item.AvailableQuantity,
                            WashCapacity = item.WashCapacity,
                            DryCapacity = item.DryCapacity,
                            Status = item.Status,
                            CreatedDate = item.CreatedDate,
                            UpdatedDate = item.UpdatedDate
                        };
                        centerResourses.Add(centerResourse);
                    }

                    response.CenterResourses = centerResourses;
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

        [Authorize(Roles = "Admin")]
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
                        var feedbackList = _feedbackService.GetAllByServiceId(item.Id);
                        int st1 = 0, st2 = 0, st3 = 0, st4 = 0, st5 = 0;
                        foreach (var feedback in feedbackList)
                        {
                            if (feedback.Rating == 1) { st1++; }
                            if (feedback.Rating == 2) { st2++; }
                            if (feedback.Rating == 3) { st3++; }
                            if (feedback.Rating == 4) { st4++; }
                            if (feedback.Rating == 5) { st5++; }
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
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Prices = servicePriceViewModels,
                            TimeEstimate = item.TimeEstimate,
                            Rating = item.Rating,
                            NumOfRating = item.NumOfRating,
                            Ratings = new int[] { st1, st2, st3, st4, st5 }
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
                    response.LastDeactivate = center.LastDeactivate;
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

        [Authorize(Roles = "Admin")]
        [HttpPut("centers/pending/{id}/approve")]
        public async Task<IActionResult> ApproveCreateCenter(int id)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    /*var response = new CenterResponseModel();
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
                        var feedbackList = _feedbackService.GetAllByServiceId(item.Id);
                        int st1 = 0, st2 = 0, st3 = 0, st4 = 0, st5 = 0;
                        foreach (var feedback in feedbackList)
                        {
                            if (feedback.Rating == 1) { st1++; }
                            if (feedback.Rating == 2) { st2++; }
                            if (feedback.Rating == 3) { st3++; }
                            if (feedback.Rating == 4) { st4++; }
                            if (feedback.Rating == 5) { st5++; }
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
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Prices = servicePriceViewModels,
                            TimeEstimate = item.TimeEstimate,
                            Rating = item.Rating,
                            NumOfRating = item.NumOfRating,
                            Ratings = new int[] { st1, st2, st3, st4, st5 }
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
                    response.LastDeactivate = center.LastDeactivate;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours;*/
                    center.Status = "Active";
                    await _centerService.Update(center);

                    string path = "./Templates_email/AcceptCenter.txt";
                    string content = System.IO.File.ReadAllText(path);
                    content = content.Replace("{recipient}", center.CreatedBy);
                    content = content.Replace("{center}", center.CenterName);
                    
                    await _sendMailService.SendEmailAsync(center.CreatedBy, "Duyệt trung tâm", content);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            centerId = center.Id
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

        [Authorize(Roles ="Admin")]
        [HttpPut("centers/pending/{id}/reject")]
        public async Task<IActionResult> RejectCreateCenter(int id)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    /*var response = new CenterResponseModel();
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
                        var feedbackList = _feedbackService.GetAllByServiceId(item.Id);
                        int st1 = 0, st2 = 0, st3 = 0, st4 = 0, st5 = 0;
                        foreach (var feedback in feedbackList)
                        {
                            if (feedback.Rating == 1) { st1++; }
                            if (feedback.Rating == 2) { st2++; }
                            if (feedback.Rating == 3) { st3++; }
                            if (feedback.Rating == 4) { st4++; }
                            if (feedback.Rating == 5) { st5++; }
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
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Prices = servicePriceViewModels,
                            TimeEstimate = item.TimeEstimate,
                            Rating = item.Rating,
                            NumOfRating = item.NumOfRating,
                            Ratings = new int[] { st1, st2, st3, st4, st5 }
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
                    response.LastDeactivate = center.LastDeactivate;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
                    response.CenterLocation = new CenterLocationResponseModel
                    {
                        Latitude = center.Location.Latitude,
                        Longitude = center.Location.Longitude
                    };
                    response.CenterOperatingHours = centerOperatingHours;*/
                    center.Status = "Rejected";
                    await _centerService.Update(center);

                    string path = "./Templates_email/RejectCenter.txt";
                    string content = System.IO.File.ReadAllText(path);
                    content = content.Replace("{recipient}", center.CreatedBy);
                    content = content.Replace("{center}", center.CenterName);

                    await _sendMailService.SendEmailAsync(center.CreatedBy, "Duyệt trung tâm", content);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            centerId = center.Id
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

        [Authorize(Roles = "Admin")]
        [HttpPut("centers/updating/{id}/approve")]
        public async Task<IActionResult> ApproveUpdateCenter(int id)
        {
            try
            {
                var center = await _centerRequestService.GetById(id);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found updating center request",
                        Data = ""
                    });
                }
                else
                {
                    var newUpdatingCenter = await _centerService.GetByIdLightWeight(center.CenterRequesting);
                    newUpdatingCenter.CenterName = center.CenterName;
                    newUpdatingCenter.Alias = center.Alias;
                    newUpdatingCenter.LocationId = center.LocationId;
                    newUpdatingCenter.Phone = center.Phone;
                    newUpdatingCenter.Description = center.Description;
                    newUpdatingCenter.MonthOff = center.MonthOff;
                    newUpdatingCenter.Image = center.Image;
                    newUpdatingCenter.TaxCode = center.TaxCode;
                    newUpdatingCenter.TaxRegistrationImage = center.TaxRegistrationImage;
                    newUpdatingCenter.Status = center.Status;
                    await _centerService.Update(newUpdatingCenter);

                    center.RequestStatus = false;
                    await _centerRequestService.Update(center);

                    string path = "./Templates_email/AcceptCenterUpdating.txt";
                    string content = System.IO.File.ReadAllText(path);
                    content = content.Replace("{recipient}", center.CreatedBy);
                    content = content.Replace("{center}", center.CenterName);

                    await _sendMailService.SendEmailAsync(center.CreatedBy, "Duyệt trung tâm", content);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            centerId = newUpdatingCenter.Id
                        }
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


        [Authorize(Roles = "Admin")]
        [HttpPut("centers/updating/{id}/reject")]
        public async Task<IActionResult> RejectUpdateCenter(int id)
        {
            try
            {
                var center = await _centerRequestService.GetById(id);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found updating center request",
                        Data = ""
                    });
                }
                else
                {
                    var newUpdatingCenter = await _centerService.GetByIdLightWeight(center.CenterRequesting);
                    newUpdatingCenter.Status = center.Status;
                    await _centerService.Update(newUpdatingCenter);

                    center.RequestStatus = false;
                    await _centerRequestService.Update(center);

                    string path = "./Templates_email/RejectCenterUpdating.txt";
                    string content = System.IO.File.ReadAllText(path);
                    content = content.Replace("{recipient}", center.CreatedBy);
                    content = content.Replace("{center}", center.CenterName);

                    await _sendMailService.SendEmailAsync(center.CreatedBy, "Duyệt trung tâm", content);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            centerId = newUpdatingCenter.Id
                        }
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
