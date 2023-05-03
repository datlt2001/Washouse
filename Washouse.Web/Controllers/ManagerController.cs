using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using System.Security.Claims;
using System.Linq;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Washouse.Model.Models;
using Washouse.Service.Implement;
//using Twilio.Http;
using static Google.Apis.Requests.BatchRequest;
using Washouse.Model.RequestModels;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using Washouse.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NuGet.Protocol;
using Washouse.Common.Helpers;
using Washouse.Common.Mails;
using Microsoft.CodeAnalysis;
using System.Data;
using System.Net.Http;
using Twilio.Rest.Chat.V1.Service;

namespace Washouse.Web.Controllers
{
    [Route("api/manager")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        #region Initialize

        private readonly ICenterService _centerService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly ILocationService _locationService;
        private readonly IWardService _wardService;
        private readonly IOperatingHourService _operatingHourService;
        private readonly IServiceService _serviceService;
        private readonly IStaffService _staffService;
        private readonly ICenterRequestService _centerRequestService;
        private readonly IFeedbackService _feedbackService;
        private readonly IPromotionService _promotionService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;
        private readonly INotificationAccountService _notificationAccountService;
        private readonly IDeliveryService _deliveryService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletService _walletService;
        private readonly IPaymentService _paymentService;
        private readonly IHubContext<MessageHub> messageHub;
        private readonly ISendMailService _sendMailService;

        public ManagerController(ICenterService centerService, ICloudStorageService cloudStorageService,
            ILocationService locationService, IWardService wardService,
            IOperatingHourService operatingHourService, IServiceService serviceService,
            IStaffService staffService, ICenterRequestService centerRequestService,
            IFeedbackService feedbackService, IPromotionService promotionService,
            INotificationService notificationService, INotificationAccountService notificationAccountService,
            ICustomerService customerService, IOrderService orderService, IAccountService accountService,
            IWalletService walletService, IWalletTransactionService walletTransactionService,
            IPaymentService paymentService,
            IHubContext<MessageHub> _messageHub, ISendMailService sendMailService, IDeliveryService deliveryService)
        {
            this._centerService = centerService;
            this._locationService = locationService;
            this._cloudStorageService = cloudStorageService;
            this._wardService = wardService;
            this._operatingHourService = operatingHourService;
            this._serviceService = serviceService;
            this._staffService = staffService;
            this._centerRequestService = centerRequestService;
            this._feedbackService = feedbackService;
            this._promotionService = promotionService;
            this._customerService = customerService;
            this._orderService = orderService;
            this._accountService = accountService;
            this._notificationService = notificationService;
            this._notificationAccountService = notificationAccountService;
            this._sendMailService = sendMailService;
            this.messageHub = _messageHub;
            this._deliveryService = deliveryService;
            this._walletService = walletService;
            this._walletTransactionService = walletTransactionService;
            this._paymentService = paymentService;
        }

        #endregion

        [Authorize(Roles = "Manager")]
        [HttpPut("my-center")]
        public async Task<IActionResult> UpdateMyCenter(CenterEditRequestModel centerEditRequest)
        {
            try
            {
                var a = int.Parse(User.FindFirst("Id")?.Value);
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                if (managerInfo.CenterId == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Account not found CenterId",
                        Data = null
                    });
                }

                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);

                var centerRequesting = await _centerService.GetByIdLightWeight(center.Id);
                if (centerRequesting == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
                        Data = null
                    });
                }

                if (center.Id != centerRequesting.Id)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Error occurs",
                        Data = null
                    });
                }

                var location = new Model.Models.Location();
                var checkLocationExistCenter = new Model.Models.Location();
                if (centerEditRequest.Location != null)
                {
                    location.AddressString = centerEditRequest.Location.AddressString;
                    location.WardId = centerEditRequest.Location.WardId;
                    /*var ward = new Ward();
                    try
                    {
                        ward = await _wardService.GetWardById(location.WardId);
                    }
                    catch
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found ward by wardId.",
                            Data = null
                        });
                    }

                    string fullAddress = centerEditRequest.Location.AddressString + ", " + ward.WardName + ", " +
                                         ward.District.DistrictName + ", TP. Hồ Chí Minh";
                    string url =
                        $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={fullAddress}&format=json&limit=1";
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                    {
                        var response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            dynamic result = JsonConvert.DeserializeObject(json);
                            if (result.Count > 0)
                            {
                                location.Latitude = result[0].lat;
                                location.Longitude = result[0].lon;
                            }
                        }
                    }*/

                    if (centerEditRequest.Location.Latitude != null && centerEditRequest.Location.Latitude != 0)
                    {
                        location.Latitude = centerEditRequest.Location.Latitude;
                    }

                    if (centerEditRequest.Location.Longitude != null && centerEditRequest.Location.Longitude != 0)
                    {
                        location.Longitude = centerEditRequest.Location.Longitude;
                    }

                    if (location.Latitude != null && location.Longitude != null && location.Latitude != 0 &&
                        location.Longitude != 0)
                    {
                        location.Latitude = Math.Round((decimal)location.Latitude, 9);
                        location.Longitude = Math.Round((decimal)location.Longitude, 9);
                    }
                    else
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message =
                                "Location of center(latitude and longitude) not recognized or not in Ho Chi Minh city.",
                            Data = null
                        });
                    }

                    var locationAdded = await _locationService.Add(location);

                    checkLocationExistCenter = await _locationService.GetById(locationAdded.Id);
                    if (checkLocationExistCenter.Centers.ToList().Count > 0)
                    {
                        var centerExist = checkLocationExistCenter.Centers.ToList();
                        foreach (var item in centerExist)
                        {
                            if (!item.Status.Equals("Closed"))
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Existing a center is operating in this location.",
                                    Data = null
                                });
                            }
                        }
                    }
                }

                var centerRequestModel = new CenterRequest();
                if (centerEditRequest != null)
                {
                    centerRequestModel.CenterRequesting = centerRequesting.Id;
                    centerRequestModel.RequestStatus = true;
                    centerRequestModel.CenterName = string.IsNullOrWhiteSpace(centerEditRequest.CenterName)
                        ? centerRequesting.CenterName
                        : centerEditRequest.CenterName.Trim();
                    centerRequestModel.Alias = string.IsNullOrWhiteSpace(centerEditRequest.Alias)
                        ? centerRequesting.Alias
                        : centerEditRequest.Alias.Trim();
                    centerRequestModel.WalletId = centerRequesting.WalletId;
                    centerRequestModel.LocationId = (checkLocationExistCenter.Id == 0)
                        ? centerRequesting.LocationId
                        : checkLocationExistCenter.Id;
                    centerRequestModel.Phone = string.IsNullOrWhiteSpace(centerEditRequest.Phone)
                        ? centerRequesting.Phone
                        : centerEditRequest.Phone.Trim();
                    centerRequestModel.Description = string.IsNullOrWhiteSpace(centerEditRequest.Description)
                        ? centerRequesting.Description
                        : centerEditRequest.Description.Trim();
                    centerRequestModel.MonthOff = string.IsNullOrWhiteSpace(centerEditRequest.MonthOff)
                        ? centerRequesting.MonthOff
                        : centerEditRequest.MonthOff.Trim();
                    centerRequestModel.IsAvailable = centerRequesting.IsAvailable;
                    centerRequestModel.Status = centerRequesting.Status;
                    centerRequestModel.Image = string.IsNullOrWhiteSpace(centerEditRequest.SavedFileName)
                        ? centerRequesting.Image
                        : centerEditRequest.SavedFileName.Trim();
                    centerRequestModel.TaxCode = centerRequesting.TaxCode;
                    centerRequestModel.TaxRegistrationImage = centerRequesting.TaxRegistrationImage;
                    centerRequestModel.HotFlag = centerRequesting.HotFlag;
                    centerRequestModel.Rating = centerRequesting.Rating;
                    centerRequestModel.NumOfRating = centerRequesting.NumOfRating;
                    centerRequestModel.HasDelivery = centerRequesting.HasDelivery;
                    centerRequestModel.HasOnlinePayment = centerRequesting.HasOnlinePayment;
                    centerRequestModel.LastDeactivate = centerRequesting.LastDeactivate;
                    centerRequestModel.CreatedDate = centerRequesting.CreatedDate;
                    centerRequestModel.CreatedBy = centerRequesting.CreatedBy;
                    centerRequestModel.UpdatedDate = DateTime.Now;
                    centerRequestModel.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                }

                await _centerRequestService.Add(centerRequestModel);
                centerRequesting.Status = "Updating";
                // Update
                await _centerService.Update(centerRequesting);

                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = centerRequestModel
                });
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

        // GET: api/manager/my-center
        [Authorize(Roles = "Manager,Staff")]
        [HttpGet("my-center")]
        public async Task<IActionResult> GetMyCenter()
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                if (managerInfo.CenterId == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Account not found CenterId",
                        Data = null
                    });
                }

                var center = await _centerService.GetMyCenter((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }

                if (center != null)
                {
                    var response = new CenterManagerResponseModel();
                    var centerOperatingHours = new List<CenterOperatingHoursResponseModel>();
                    var centerDeliveryPrices = new List<CenterDeliveryPriceChartResponseModel>();
                    var centerAdditionServices = new List<AdditionServiceCenterModel>();
                    var centerGalleries = new List<CenterGalleryModel>();
                    var centerFeedbacks = new List<FeedbackCenterModel>();
                    var centerResourses = new List<ResourseCenterModel>();
                    /*foreach (var item in center.Services)
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
                            Prices = servicePriceViewModels.OrderByDescending(a => a.Price).ToList(),
                            TimeEstimate = item.TimeEstimate,
                            Rating = item.Rating,
                            NumOfRating = item.NumOfRating,
                            Ratings = new int[] { st1, st2, st3, st4, st5 }
                        };
                        servicesOfCenter.Add(service);
                    }
                    foreach (var service in center.Services)
                    {
                        if (service.PriceType)
                        {
                            foreach (var servicePrice in service.ServicePrices)
                            {
                                if (minPrice > service.MinPrice || minPrice == 0)
                                {
                                    minPrice = (decimal)service.MinPrice;
                                }
                                if (maxPrice < (servicePrice.Price * servicePrice.MaxValue) || maxPrice == 0)
                                {
                                    maxPrice = (decimal)(servicePrice.Price * servicePrice.MaxValue);
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
                            Services = servicesOfCenter.Where(ser => ser.CategoryId == service.CategoryId).ToList()
                        };
                        if (centerServices.FirstOrDefault(cs => cs.ServiceCategoryID == centerService.ServiceCategoryID) == null) centerServices.Add(centerService);
                    }
                    */
                    //int nowDayOfWeek = ((int)DateTime.Today.DayOfWeek != 0) ? (int)DateTime.Today.DayOfWeek : 8;
                    //if (center.OperatingHours.FirstOrDefault(a => a.DaysOfWeekId == nowDayOfWeek) != null)
                    //{
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
                    response.Thumbnail = center.Image != null
                        ? await _cloudStorageService.GetSignedUrlAsync(center.Image)
                        : null;
                    response.Title = center.CenterName;
                    response.Alias = center.Alias;
                    response.Description = center.Description;
                    response.Rating = center.Rating;
                    response.NumOfRating = center.NumOfRating;
                    response.Phone = center.Phone;
                    response.CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName +
                                             ", " + center.Location.Ward.District.DistrictName;
                    //response.Distance = distance;
                    response.IsAvailable = center.IsAvailable;
                    response.Status = center.Status;
                    response.TaxCode = center.TaxCode.Trim();
                    response.TaxRegistrationImage = center.TaxRegistrationImage;
                    response.MonthOff = MonthOff;
                    response.HasDelivery = center.HasDelivery;
                    response.HasOnlinePayment = center.HasOnlinePayment;
                    response.LocationId = center.LocationId;
                    response.CenterDeliveryPrices = centerDeliveryPrices;
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

        [Authorize(Roles = "Manager")]
        // GET: api/manager/services
        [HttpGet("services")]
        public async Task<IActionResult> GetServicesOfCenterManaged(
            [FromQuery] FilterServicesOfCenterRequestModel filter)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }


                var services = await _serviceService.GetAllByCenterId(center.Id);
                if (services == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found service of your center.",
                        Data = null
                    });
                }

                if (services != null)
                {
                    if (!filter.SearchString.IsNullOrEmpty())
                    {
                        services = services.Where(service =>
                                (service.ServiceName.ToLower().Contains(filter.SearchString.ToLower()))
                                || (service.Alias.ToLower().Contains(filter.SearchString.ToLower())))
                            .ToList();
                    }

                    var servicesOfCenter = new List<ServiceCenterModel>();

                    foreach (var item in services)
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
                            if (feedback.Rating == 1)
                            {
                                st1++;
                            }

                            if (feedback.Rating == 2)
                            {
                                st2++;
                            }

                            if (feedback.Rating == 3)
                            {
                                st3++;
                            }

                            if (feedback.Rating == 4)
                            {
                                st4++;
                            }

                            if (feedback.Rating == 5)
                            {
                                st5++;
                            }
                        }

                        var itemResponse = new ServiceCenterModel
                        {
                            ServiceId = item.Id,
                            ServiceName = item.ServiceName,
                            Alias = item.Alias,
                            CategoryId = item.CategoryId,
                            CategoryName = item.Category.CategoryName,
                            Description = item.Description,
                            Image = item.Image != null
                                ? await _cloudStorageService.GetSignedUrlAsync(item.Image)
                                : null,
                            PriceType = item.PriceType,
                            Price = item.Price,
                            MinPrice = item.MinPrice,
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Prices = servicePriceViewModels,
                            TimeEstimate = item.TimeEstimate,
                            IsAvailable = item.IsAvailable,
                            Status = item.Status,
                            HomeFlag = item.HomeFlag,
                            HotFlag = item.HotFlag,
                            Rating = item.Rating,
                            NumOfRating = item.NumOfRating,
                            Ratings = new int[] { st1, st2, st3, st4, st5 }
                        };
                        servicesOfCenter.Add(itemResponse);
                    }

                    int totalItems = servicesOfCenter.Count();
                    if (filter.PageSize == -1)
                    {
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = new
                            {
                                TotalItems = totalItems,
                                TotalPages = 0,
                                ItemsPerPage = -1,
                                PageNumber = 0,
                                Items = servicesOfCenter
                            }
                        });
                    }


                    int totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);
                    servicesOfCenter = servicesOfCenter
                        .Skip((filter.Page - 1) * filter.PageSize)
                        .Take(filter.PageSize).ToList();

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            ItemsPerPage = filter.PageSize,
                            PageNumber = filter.Page,
                            Items = servicesOfCenter
                        }
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found services",
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

        [Authorize(Roles = "Manager")]
        // GET: api/manager/promotions
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotionsOfCenterManaged()
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }

                if (center != null)
                {
                    var promotion = _promotionService.GetAllByCenterId((int)managerInfo.CenterId);
                    if (promotion == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found promotion of your center.",
                            Data = null
                        });
                    }

                    if (promotion != null)
                    {
                        var promotionResponses = new List<PromotionCenterModel>();
                        foreach (var item in promotion)
                        {
                            string _startDate = null;
                            string _expireDate = null;
                            string _updatedDate = null;
                            if (item.StartDate.HasValue)
                            {
                                _startDate = item.StartDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                            }

                            if (item.ExpireDate.HasValue)
                            {
                                _expireDate = item.ExpireDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                            }

                            if (item.UpdatedDate.HasValue)
                            {
                                _updatedDate = item.UpdatedDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                            }

                            var itemResponse = new PromotionCenterModel
                            {
                                Id = item.Id,
                                Code = item.Code,
                                Description = item.Description,
                                Discount = item.Discount,
                                StartDate = _startDate,
                                ExpireDate = _expireDate,
                                CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                                UpdatedDate = _updatedDate,
                                UseTimes = item.UseTimes,
                                IsAvailable = item.Status
                            };
                            promotionResponses.Add(itemResponse);
                        }

                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = promotionResponses
                        });
                    }
                    else
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found promotions",
                            Data = null
                        });
                    }
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
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

        [Authorize(Roles = "Manager")]
        // POST: api/manager/promotions
        [HttpPost("promotions")]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionRequestModel Input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DateTime StartDate;
                    DateTime ExpireDate;
                    if (!string.IsNullOrEmpty(Input.StartDate) && DateTime.TryParseExact(Input.StartDate, "dd-MM-yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out StartDate))
                    {
                        try
                        {
                            StartDate = DateTime.ParseExact(Input.StartDate, "dd-MM-yyyy",
                                CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "StartDate: " + ex.Message,
                                Data = null
                            });
                        }
                    }
                    else
                    {
                        StartDate = DateTime.Now;
                    }

                    if (!string.IsNullOrEmpty(Input.ExpireDate) && DateTime.TryParseExact(Input.ExpireDate,
                            "dd-MM-yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out ExpireDate))
                    {
                        try
                        {
                            ExpireDate = DateTime.ParseExact(Input.ExpireDate, "dd-MM-yyyy",
                                CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "ExpireDate: " + ex.Message,
                                Data = null
                            });
                        }
                    }
                    else
                    {
                        ExpireDate = StartDate.AddMonths(3);
                    }

                    var promotion = new Promotion()
                    {
                        Code = Input.Code,
                        Description = Input.Description,
                        Discount = Input.Discount,
                        StartDate = StartDate,
                        ExpireDate = ExpireDate.AddDays(1).AddSeconds(-1),
                        UseTimes = Input.UseTimes,
                        CenterId = int.Parse(User.FindFirst("CenterManaged")?.Value),
                        CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                        CreatedDate = DateTime.Now
                    };
                    await _promotionService.Add(promotion);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            PromotionId = promotion.Id,
                            PromotionCode = promotion.Code
                        }
                    });
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

        [Authorize(Roles = "Manager")]
        // PUT: api/manager/promotions
        [HttpPut("promotions")]
        public async Task<IActionResult> UpdatePromotion(int PromotionId, [FromBody] UpdatePromotionRequestModel Input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var promotion = await _promotionService.GetById(PromotionId);
                    if (promotion == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found promotion",
                            Data = ""
                        });
                    }

                    DateTime StartDate;
                    DateTime ExpireDate;
                    if (Input.StartDate != null)
                    {
                        try
                        {
                            StartDate = DateTime.ParseExact(Input.StartDate, "dd-MM-yyyy",
                                CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "StartDate: " + ex.Message,
                                Data = null
                            });
                        }

                        promotion.StartDate = StartDate;
                    }

                    if (Input.ExpireDate != null)
                    {
                        try
                        {
                            ExpireDate = DateTime.ParseExact(Input.ExpireDate, "dd-MM-yyyy",
                                CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "ExpireDate: " + ex.Message,
                                Data = ""
                            });
                        }

                        promotion.ExpireDate = ExpireDate.AddDays(1).AddSeconds(-1);
                    }

                    if (Input.UseTimes != null)
                    {
                        if ((int)Input.UseTimes < 1)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "UseTimes must be an integer more than or equal 1.",
                                Data = ""
                            });
                        }

                        promotion.UseTimes = Input.UseTimes;
                    }

                    if (Input.StartDate != null && Input.ExpireDate != null)
                    {
                        if (promotion.ExpireDate <= promotion.StartDate)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "ExpireDate must be more than StartDate.",
                                Data = ""
                            });
                        }
                    }

                    promotion.UpdatedDate = DateTime.Now;
                    promotion.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    await _promotionService.Update(promotion);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            PromotionId = promotion.Id,
                            PromotionCode = promotion.Code
                        }
                    });
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

        [Authorize(Roles = "Manager")]
        // PUT: api/manager/promotions
        [HttpPut("promotions/activate")]
        public async Task<IActionResult> ActivatePromotion(int PromotionId, string ExpireDate, int? UseTimes)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var promotion = await _promotionService.GetById(PromotionId);
                    if (promotion == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found promotion",
                            Data = ""
                        });
                    }

                    DateTime ExpiredDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(ExpireDate) && DateTime.TryParseExact(ExpireDate, "dd-MM-yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out ExpiredDate))
                    {
                        try
                        {
                            ExpiredDate = DateTime.ParseExact(ExpireDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "ExpireDate: " + ex.Message,
                                Data = null
                            });
                        }
                    }

                    if (ExpiredDate.AddDays(1).AddSeconds(-1)
                            .CompareTo(DateTime.Today) < 0)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Expired date must after today",
                            Data = null
                        });
                    }

                    promotion.Status = true;
                    promotion.ExpireDate = string.IsNullOrEmpty(ExpireDate)
                        ? promotion.ExpireDate
                        : ExpiredDate.AddDays(1).AddSeconds(-1);
                    promotion.StartDate = DateTime.Now;
                    promotion.UseTimes = UseTimes ?? promotion.UseTimes;
                    promotion.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    promotion.UpdatedDate = DateTime.Now;
                    await _promotionService.Update(promotion);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            PromotionId = promotion.Id,
                            PromotionCode = promotion.Code
                        }
                    });
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

        [Authorize(Roles = "Manager")]
        // PUT: api/manager/promotions
        [HttpPut("promotions/deactivate")]
        public async Task<IActionResult> DeactivatePromotion(int PromotionId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var promotion = await _promotionService.GetById(PromotionId);
                    if (promotion == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found promotion",
                            Data = ""
                        });
                    }

                    promotion.Status = false;
                    promotion.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    promotion.UpdatedDate = DateTime.Now;
                    await _promotionService.Update(promotion);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            PromotionId = promotion.Id,
                            PromotionCode = promotion.Code
                        }
                    });
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


        [Authorize(Roles = "Manager,Staff")]
        // GET: api/manager/my-center/customers
        [HttpGet("my-center/customers")]
        public async Task<IActionResult> GetCustomerOfCenterManaged([FromQuery] FilterCustomersOfCenterRequestModel filter)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }

                if (center != null)
                {
                    var customersOfCenter = await _customerService.CustomersOfCenter(center.Id);
                    if (customersOfCenter == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found any customer of your center.",
                            Data = null
                        });
                    }

                    if (customersOfCenter != null)
                    {
                        if (!filter.SearchString.IsNullOrEmpty())
                        {
                            customersOfCenter = customersOfCenter.Where(cus =>
                                    (cus.Fullname.ToLower().Contains(filter.SearchString.ToLower()))
                                    || (cus.Phone.ToLower().Contains(filter.SearchString.ToLower())))
                                .ToList();
                        }

                        var customers = new List<CustomerCenterModel>();
                        foreach (var item in customersOfCenter)
                        {
                            string addressStringResponse = null;
                            if (item.Address != null)
                            {
                                //var location = await _locationService.GetByIdIncludeWardDistrict(item.Address.Value);
                                var location = item.AddressNavigation;
                                addressStringResponse = location.AddressString + ", " + location.Ward.WardName + ", " +
                                                        location.Ward.District.DistrictName + ", " +
                                                        "TP. Hồ Chí Minh";
                            }

                            string dob = null;
                            int? gender = null;
                            if (item.AccountId != null)
                            {
                                //var account = await _accountService.GetByIdLightWeight(item.AccountId.Value);
                                var account = item.Account;
                                dob = account.Dob.HasValue ? (account.Dob.Value).ToString("dd-MM-yyyy HH:mm:ss") : null;
                                gender = account.Gender;
                            }

                            var itemResponse = new CustomerCenterModel
                            {
                                Id = item.Id,
                                AccountId = item.AccountId,
                                Fullname = item.Fullname,
                                Phone = item.Phone,
                                Email = item.Email,
                                AddressString = addressStringResponse,
                                DateOfBirth = dob,
                                Gender = gender
                            };
                            customers.Add(itemResponse);
                        }

                        int totalItems = customers.Count();
                        if (filter.Pagination != null && filter.Pagination.PageSize == -1)
                        {
                            return Ok(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "success",
                                Data = new
                                {
                                    TotalItems = totalItems,
                                    TotalPages = 0,
                                    ItemsPerPage = -1,
                                    PageNumber = 0,
                                    Items = customers
                                }
                            });
                        }
                        else if (filter.Pagination == null)
                        {
                            filter.Pagination = new PaginationViewModel { Page = 1, PageSize = 10 };
                        }

                        int totalPages = (int)Math.Ceiling((double)totalItems / filter.Pagination.PageSize);
                        customers = customers.Skip((filter.Pagination.Page - 1) * filter.Pagination.PageSize)
                            .Take(filter.Pagination.PageSize).ToList();

                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = new
                            {
                                TotalItems = totalItems,
                                TotalPages = totalPages,
                                ItemsPerPage = filter.Pagination.PageSize,
                                PageNumber = filter.Pagination.Page,
                                Items = customers
                            }
                        });
                    }
                    else
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found customers",
                            Data = null
                        });
                    }
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
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

        [Authorize(Roles = "Manager,Staff")]
        /// <summary>
        /// Gets the list of all Orders.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/orders
        ///     {        
        ///       "page": 1,
        ///       "pageSize": 5,
        ///       "searchString": "Dr"  
        ///     }
        /// </remarks>
        /// <returns>The list of Centers.</returns>
        /// <response code="200">Success return list orders</response>   
        /// <response code="404">Not found any order matched</response>   
        /// <response code="400">One or more error occurs</response>   
        /// <response code="401">Unauthorization</response>   
        // GET: api/manager/my-center/orders
        [HttpGet("my-center/orders")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> GetOrdersOfCenter(
            [FromQuery] FilterOrdersRequestModel filterOrdersRequestModel)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }
                else
                {
                    var orders = await _orderService.GetOrdersOfCenter(center.Id);
                    if (filterOrdersRequestModel.SearchString != null)
                    {
                        orders = orders.Where(order => (order.OrderDetails.Any(orderDetail =>
                                                            (orderDetail.Service.ServiceName.ToLower()
                                                                 .Contains(
                                                                     filterOrdersRequestModel.SearchString.ToLower())
                                                             || (orderDetail.Service.Alias != null &&
                                                                 orderDetail.Service.Alias.ToLower()
                                                                     .Contains(filterOrdersRequestModel.SearchString
                                                                         .ToLower()))))
                                                        || order.Id.ToLower()
                                                            .Contains(filterOrdersRequestModel.SearchString.ToLower())
                                                        || order.CustomerEmail.ToLower()
                                                            .Contains(filterOrdersRequestModel.SearchString.ToLower())
                                                        || (order.Customer != null && order.Customer.Fullname.ToLower()
                                                            .Contains(filterOrdersRequestModel.SearchString.ToLower()))
                                                        || order.CustomerName.ToLower()
                                                            .Contains(filterOrdersRequestModel.SearchString.ToLower())))
                            .ToList();
                    }

                    if (filterOrdersRequestModel.Status != null)
                    {
                        orders = orders.Where(order =>
                            order.Status.ToLower().Equals(filterOrdersRequestModel.Status.ToLower()));
                    }

                    if (filterOrdersRequestModel.FromDate != null)
                    {
                        DateTime dateValue;
                        bool success = DateTime.TryParseExact(filterOrdersRequestModel.FromDate, "dd-MM-yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue);

                        orders = orders.Where(order => (order.CreatedDate >= dateValue));
                    }

                    if (filterOrdersRequestModel.ToDate != null)
                    {
                        DateTime dateValue;
                        bool success = DateTime.TryParseExact(filterOrdersRequestModel.ToDate, "dd-MM-yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue);

                        orders = orders.Where(order => (order.CreatedDate <= dateValue.AddDays(1)));
                    }

                    if (filterOrdersRequestModel.DeliveryType == true)
                    {
                        orders = orders.Where(
                            order => order.DeliveryType != 0);
                    }

                    if (filterOrdersRequestModel.DeliveryStatus != null)
                    {
                        orders = orders.Where(order => order.Deliveries.Any(delivery =>
                            Equals(delivery.Status.ToLower(), filterOrdersRequestModel.DeliveryStatus.ToLower())));
                    }


                    orders = orders.OrderByDescending(x => x.CreatedDate).ToList();
                    int totalItems = orders.Count();

                    int totalPages = (int)Math.Ceiling((double)totalItems / filterOrdersRequestModel.PageSize);

                    if (filterOrdersRequestModel.PageSize != -1)
                    {
                        orders = orders.Skip((filterOrdersRequestModel.Page - 1) * filterOrdersRequestModel.PageSize)
                            .Take(filterOrdersRequestModel.PageSize).ToList();
                    }

                    var response = new List<OrderCenterModel>();

                    foreach (var order in orders)
                    {
                        decimal TotalOrderValue = 0;
                        var orderedServices = new List<OrderedServiceModel>();
                        foreach (var item in order.OrderDetails)
                        {
                            TotalOrderValue += item.Price;
                            orderedServices.Add(new OrderedServiceModel
                            {
                                ServiceName = item.Service.ServiceName,
                                Measurement = item.Measurement,
                                Unit = item.Service.Unit,
                                Image = item.Service.Image != null
                                    ? await _cloudStorageService.GetSignedUrlAsync(item.Service.Image)
                                    : null,
                                ServiceCategory = item.Service.Category.CategoryName,
                                Price = item.Price
                            });
                        }

                        string _orderDate = null;
                        if (order.CreatedDate.HasValue)
                        {
                            _orderDate = order.CreatedDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                        }


                        response.Add(new OrderCenterModel
                        {
                            OrderId = order.Id,
                            OrderDate = _orderDate,
                            CustomerName = order.CustomerName,
                            TotalOrderValue = TotalOrderValue,
                            DeliveryType = order.DeliveryType,
                            Discount = order.Payments.Count > 0 ? order.Payments.First().Discount : 0,
                            TotalOrderPayment = order.Payments.Count > 0 ? order.Payments.First().Total : 0,
                            Status = order.Status,
                            IsFeedback = order.IsFeedback,
                            IsPayment = ((order.Payments != null && order.Payments.Count() > 0) && (order.Payments.FirstOrDefault().Status.Trim().ToLower().Equals("paid") ||
                                            order.Payments.FirstOrDefault().Status.Trim().ToLower().Equals("received"))),
                            CenterId = center.Id,
                            CenterName = center.CenterName,
                            Deliveries = order.Deliveries.ToList().ConvertAll(delivery => new OrderedDeliveryModel
                            {
                                DeliveryStatus = delivery.Status,
                                DeliveryType = delivery.DeliveryType,
                                AddressString = delivery.Location?.AddressString,
                                DistrictName = delivery.Location?.Ward?.District?.DistrictName,
                                WardName = delivery.Location?.Ward?.WardName
                            }),
                            OrderedServices = orderedServices
                        });
                    }

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            ItemsPerPage = filterOrdersRequestModel.PageSize,
                            PageNumber = filterOrdersRequestModel.Page,
                            Items = response
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


        [Authorize(Roles = "Manager,Staff")]
        /// <summary>
        /// Gets the list of all Orders.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/orders
        ///     {        
        ///       "page": 1,
        ///       "pageSize": 5,
        ///       "searchString": "Dr"  
        ///     }
        /// </remarks>
        /// <returns>The list of Centers.</returns>
        /// <response code="200">Success return list orders</response>   
        /// <response code="404">Not found any order matched</response>   
        /// <response code="400">One or more error occurs</response>   
        /// <response code="401">Unauthorization</response>   
        // GET: api/manager/my-center/orders/{id}
        [HttpGet("my-center/orders/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> GetOrderDetailOfCenterOrder(string id)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }
                else
                {
                    var order = await _orderService.GetOrderByIdCenterManaged(id);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = null
                        });
                    }
                    if (center.Id != order.OrderDetails.First().Service.CenterId)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Not have permission",
                            Data = null
                        });
                    }

                    var response = new OrderInfomationModel();
                    response.Id = id;
                    response.CustomerName = order.CustomerName;
                    response.LocationId = order.LocationId;
                    response.CustomerAddress = order.Location.AddressString + ", " + order.Location.Ward.WardName +
                                               ", " + order.Location.Ward.District.DistrictName +
                                               ", TP. Hồ Chí Minh";
                    response.CustomerEmail = order.CustomerEmail;
                    response.CustomerMobile = order.CustomerMobile;
                    response.CustomerMessage = order.CustomerMessage;
                    response.CustomerOrdered = order.CustomerId;
                    response.DeliveryType = order.DeliveryType;
                    response.DeliveryPrice = order.DeliveryPrice;
                    response.CancelReasonByCustomer = order.CancelReasonByCustomer;
                    response.CancelReasonByStaff = order.CancelReasonByStaff;
                    response.PreferredDropoffTime = order.PreferredDropoffTime.HasValue
                        ? (order.PreferredDropoffTime.Value).ToString("dd-MM-yyyy HH:mm:ss")
                        : null;
                    response.PreferredDeliverTime = order.PreferredDeliverTime.HasValue
                        ? (order.PreferredDeliverTime.Value).ToString("dd-MM-yyyy HH:mm:ss")
                        : null;
                    response.Status = order.Status;
                    var OrderedDetails = new List<OrderDetailInfomationModel>();
                    var OrderTrackings = new List<OrderTrackingModel>();
                    var OrderDeliveries = new List<OrderDeliveryModel>();
                    var OrderPayment = new OrderPaymentModel()
                    {
                        PaymentTotal = order.Payments.First().Total,
                        PlatformFee = order.Payments.First().PlatformFee,
                        DateIssue = order.Payments.First().Date.HasValue
                            ? (order.Payments.First().Date.Value).ToString("dd-MM-yyyy HH:mm:ss")
                            : null,
                        Status = order.Payments.First().Status,
                        PaymentMethod = order.Payments.First().PaymentMethod,
                        PromoCode = order.Payments.First().PromoCodeNavigation != null
                            ? order.Payments.First().PromoCodeNavigation.Code
                            : null,
                        Discount = order.Payments.First().Discount,
                        CreatedDate = order.Payments.First().CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                        UpdatedDate = order.Payments.First().UpdatedDate.HasValue
                            ? (order.Payments.First().UpdatedDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                            : null
                    };
                    response.OrderPayment = OrderPayment;
                    decimal TotalOrderValue = 0;
                    foreach (var item in order.OrderDetails)
                    {
                        TotalOrderValue += item.Price;
                        var _orderDetailTrackingModel = new List<OrderDetailTrackingModel>();
                        foreach (var tracking in item.OrderDetailTrackings)
                        {
                            _orderDetailTrackingModel.Add(new OrderDetailTrackingModel
                            {
                                Status = tracking.Status,
                                CreatedDate = tracking.CreatedDate.HasValue
                                    ? (tracking.CreatedDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                                    : null,
                                UpdatedDate = tracking.UpdatedDate.HasValue
                                    ? (tracking.UpdatedDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                                    : null,
                            });
                        }

                        OrderedDetails.Add(new OrderDetailInfomationModel
                        {
                            OrderDetailId = item.Id,
                            ServiceName = item.Service.ServiceName,
                            ServiceCategory = item.Service.Category.CategoryName,
                            Measurement = item.Measurement,
                            Unit = item.Service.Unit,
                            CustomerNote = item.CustomerNote,
                            StaffNote = item.StaffNote,
                            Status = item.Status,
                            Image = item.Service.Image != null
                                ? await _cloudStorageService.GetSignedUrlAsync(item.Service.Image)
                                : null,
                            Price = item.Price,
                            UnitPrice = item.Price / item.Measurement,
                            OrderDetailTrackings = _orderDetailTrackingModel
                        });
                    }

                    response.TotalOrderValue = TotalOrderValue;
                    response.OrderedDetails = OrderedDetails;
                    foreach (var tracking in order.OrderTrackings)
                    {
                        OrderTrackings.Add(new OrderTrackingModel
                        {
                            Status = tracking.Status,
                            CreatedDate = tracking.CreatedDate.HasValue
                                ? (tracking.CreatedDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                                : null,
                            UpdatedDate = tracking.UpdatedDate.HasValue
                                ? (tracking.UpdatedDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                                : null,
                        });
                    }

                    response.OrderTrackings = OrderTrackings;
                    foreach (var delivery in order.Deliveries)
                    {
                        var location = await _locationService.GetByIdIncludeWardDistrict(delivery.LocationId);
                        OrderDeliveries.Add(new OrderDeliveryModel
                        {
                            ShipperName = delivery.ShipperName,
                            ShipperPhone = delivery.ShipperPhone,
                            LocationId = delivery.LocationId,
                            AddressString = location.AddressString + ", " + location.Ward.WardName + ", " +
                                            location.Ward.District.DistrictName + ", TP. Hồ Chí Minh",
                            DeliveryType = delivery.DeliveryType,
                            EstimatedTime = delivery.EstimatedTime,
                            Status = delivery.Status,
                            DeliveryDate = delivery.DeliveryDate.HasValue
                                ? (delivery.DeliveryDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                                : null
                        });
                    }

                    response.OrderDeliveries = OrderDeliveries;
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = response
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

        [Authorize(Roles = "Manager,Staff")]
        [HttpPut("my-center/orders/{id}/paid")]
        public async Task<IActionResult> OrderPaid(string id)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }
                else
                {
                    var order = await _orderService.GetOrderWithPayment(id);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = null
                        });
                    }
                    if (center.Id != order.OrderDetails.First().Service.CenterId)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Not have permission",
                            Data = null
                        });
                    }
                    var payment = order.Payments.FirstOrDefault();
                    if (payment.Status.Trim().ToLower().Equals("paid") || payment.Status.Trim().ToLower().Equals("received"))
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Order has already paid",
                            Data = null
                        });
                    }
                    if (payment.PaymentMethod != 0) 
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Payment method is not cash",
                            Data = null
                        });
                    }
                    payment.Status = "Paid";
                    payment.Date = DateTime.Now;
                    payment.UpdatedDate = DateTime.Now;
                    payment.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    await _paymentService.Update(payment);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            OrderId = order.Id
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


        [Authorize(Roles = "Staff,Manager")]
        // GET: api/manager/my-center/orders/{orderId}/details/{orderDetailId}
        [HttpPut("my-center/orders/{orderId}/details/{orderDetailId}")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateOrderDetail(string orderId, int orderDetailId,
            [FromBody] UpdateOrderDetailRequestModel updateModel)
        {
            try
            {
                if (updateModel.Measurement == null && updateModel.StaffNote == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Not update",
                        Data = null
                    });
                }

                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
                        Data = ""
                    });
                }
                else
                {
                    var order = await _orderService.GetOrderByIdToUpdateOrderDetail(orderId);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = ""
                        });
                    }

                    if (order.Status.Trim().ToLower().Equals("completed"))
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Order has been already completed",
                            Data = null
                        });
                    }

                    var orderDetail = order.OrderDetails.FirstOrDefault(orderDetail => orderDetail.Id == orderDetailId);
                    if (orderDetail == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order detail",
                            Data = ""
                        });
                    }

                    var payment = order.Payments.FirstOrDefault();
                    if (updateModel.Measurement != null)
                    {
                        orderDetail.Measurement = (decimal)updateModel.Measurement;
                        var service = orderDetail.Service;

                        decimal totalCurrentPrice = 0;
                        decimal currentPrice = 0;
                        if (service.PriceType)
                        {
                            var priceChart = service.ServicePrices.OrderBy(a => a.MaxValue).ToList();
                            bool check = false;
                            foreach (var itemSerivePrice in priceChart)
                            {
                                if (updateModel.Measurement <= itemSerivePrice.MaxValue && !check)
                                {
                                    currentPrice = itemSerivePrice.Price;
                                }

                                if (currentPrice > 0)
                                {
                                    check = true;
                                }
                            }

                            if (currentPrice * updateModel.Measurement < service.MinPrice)
                            {
                                totalCurrentPrice = (decimal)service.MinPrice;
                            }
                            else
                            {
                                totalCurrentPrice = currentPrice * (decimal)updateModel.Measurement;
                            }
                        }
                        else
                        {
                            totalCurrentPrice = (decimal)service.Price * (decimal)updateModel.Measurement;
                        }

                        //payment
                        if (payment.Discount != null)
                        {
                            decimal total = (payment.Total / (1 - (decimal)payment.Discount) - orderDetail.Price +
                                            totalCurrentPrice) * (1 - (decimal)payment.Discount);
                            int roundedTotal = (int)Math.Round(total);
                            if (total > 0)
                            {
                                payment.Total = roundedTotal;
                            }
                            else
                            {
                                payment.Total = 0;
                            }
                        }
                        else
                        {
                            decimal total = payment.Total - orderDetail.Price + totalCurrentPrice;
                            if (total > 0)
                            {
                                payment.Total = total;
                            }
                            else
                            {
                                payment.Total = 0;
                            }
                        }

                        payment.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                        payment.UpdatedDate = DateTime.Now;
                        orderDetail.Price = totalCurrentPrice;
                    }

                    if (updateModel.StaffNote != null)
                    {
                        orderDetail.StaffNote = updateModel.StaffNote;
                    }

                    await _orderService.UpdateOrderDetail(orderDetail, payment);
                    //notification
                    string id = User.FindFirst("Id")?.Value;
                    Notification notification = new Notification();
                    NotificationAccount notificationAccount = new NotificationAccount();
                    notification.OrderId = order.Id;
                    notification.CreatedDate = DateTime.Now;
                    notification.Title = "Thông báo về đơn hàng:  " + order.Id;
                    notification.Content = "Đơn hàng " + order.Id + " đã được cập nhật.";
                    await _notificationService.Add(notification);

                    if (order.Customer.AccountId != null)
                    {
                        //var cusinfo = _customerService.GetById(orderAdded.CustomerId);
                        //var accId = cusinfo.Result.AccountId ?? 0;
                        notificationAccount.AccountId = (int)order.Customer.AccountId;
                        notificationAccount.NotificationId = notification.Id;
                        await _notificationAccountService.Add(notificationAccount);
                    }

                    var staff = _staffService.GetAllByCenterId(center.Id);
                    if (staff != null)
                    {
                        foreach (var staffItem in staff)
                        {
                            notificationAccount.AccountId = staffItem.AccountId;
                            notificationAccount.NotificationId = notification.Id;
                            await _notificationAccountService.Add(notificationAccount);
                        }
                    }

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            orderId = order.Id
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

        /// <summary>
        /// Create a service.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/services
        ///       {
        ///       serviceName: "Giặt sấy hấp ủi",
        ///       alias: "Giat say hap ui",
        ///       serviceCategory: 1,
        ///       serviceDescription: "Giặt rồi sấy rồi hấp xong đem ủi",
        ///       serviceImage: "test.png",
        ///       timeEstimate: 310,
        ///       unit: "Kg",
        ///       rate: 1,
        ///       priceType: true,
        ///       price: 15000,
        ///       minPrice: 40000,
        ///       serviceGalleries: [
        ///       galleries1.png, "galleries2.png"
        ///       ],
        ///       prices: [
        ///       {
        ///       maxValue: 4,
        ///       price: 15000
        ///       },
        ///       {
        ///       maxValue: 6,
        ///       price: 12000
        ///       }
        ///       ]
        ///       }
        ///   
        /// </remarks>
        /// 
        /// <returns>Center created.</returns>
        /// <response code="200">Success create a ceter</response>     
        /// <response code="400">One or more error occurs</response>   
        // POST: api/centers
        [Authorize(Roles = "Manager")]
        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] ServiceRequestModel serviceRequestmodel)
        {
            try
            {
                Model.Models.Service serviceRequest = new Model.Models.Service();
                List<ServicePrice> prices = new List<ServicePrice>();
                List<ServiceGallery> galleries = new List<ServiceGallery>();
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(User.FindFirst("CenterManaged")?.Value) ||
                        int.Parse(User.FindFirst("CenterManaged")?.Value) == 0)
                    {
                        return BadRequest();
                    }

                    serviceRequest.ServiceName = serviceRequestmodel.ServiceName;
                    serviceRequest.Alias = serviceRequestmodel.Alias;
                    serviceRequest.CategoryId = serviceRequestmodel.ServiceCategory;
                    serviceRequest.Description = serviceRequestmodel.ServiceDescription;
                    serviceRequest.PriceType = serviceRequestmodel.PriceType;
                    serviceRequest.Image = serviceRequestmodel.ServiceImage;
                    if (!serviceRequest.PriceType)
                    {
                        serviceRequestmodel.Prices = null;
                        serviceRequest.Price = serviceRequestmodel.Price;
                    }

                    serviceRequest.MinPrice = serviceRequestmodel.MinPrice;
                    serviceRequest.TimeEstimate = serviceRequestmodel.TimeEstimate;
                    serviceRequest.Unit = serviceRequestmodel.Unit;
                    serviceRequest.Rate = serviceRequestmodel.Rate;
                    serviceRequest.Status = "Active";
                    serviceRequest.HomeFlag = false;
                    serviceRequest.HotFlag = false;
                    serviceRequest.Rating = null;
                    serviceRequest.NumOfRating = 0;
                    serviceRequest.CreatedDate = DateTime.Now;
                    serviceRequest.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    serviceRequest.UpdatedDate = null;
                    serviceRequest.UpdatedBy = null;
                    serviceRequest.CenterId = int.Parse(User.FindFirst("CenterManaged")?.Value);

                    //Add Prices
                    if (serviceRequest.PriceType)
                    {
                        List<ServicePriceViewModel> servicePrices =
                            JsonConvert.DeserializeObject<List<ServicePriceViewModel>>(
                                serviceRequestmodel.Prices.ToJson());
                        if (servicePrices.Count > 0)
                        {
                            servicePrices = servicePrices.OrderBy(sp => sp.MaxValue).ToList();
                            var firstLoop = true;
                            foreach (var item in servicePrices)
                            {
                                if (firstLoop && serviceRequest.MinPrice == null)
                                {
                                    serviceRequest.MinPrice = item.MaxValue * item.Price;
                                    firstLoop = false;
                                }

                                var servicePrice = new ServicePrice();
                                servicePrice.MaxValue = item.MaxValue;
                                servicePrice.Price = item.Price;
                                servicePrice.CreatedDate = DateTime.Now;
                                servicePrice.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                                servicePrice.UpdatedDate = null;
                                servicePrice.UpdatedBy = null;

                                prices.Add(servicePrice);
                            }
                        }
                    }

                    //Add Galleries
                    List<string> serviceGalleries =
                        JsonConvert.DeserializeObject<List<string>>(serviceRequestmodel.ServiceGalleries.ToJson());
                    if (serviceGalleries.Count > 0)
                    {
                        foreach (var item in serviceGalleries)
                        {
                            var gallery = new ServiceGallery();
                            gallery.Image = item;
                            gallery.CreatedDate = DateTime.Now;
                            gallery.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;

                            galleries.Add(gallery);
                        }
                    }

                    var result = await _serviceService.Create(serviceRequest, prices, galleries);
                    List<ServicePriceViewModel> servicePriceList =
                        JsonConvert.DeserializeObject<List<ServicePriceViewModel>>(serviceRequestmodel.Prices.ToJson());
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new ServicesOfCenterResponseModel
                        {
                            ServiceId = result.Id,
                            CategoryId = result.CategoryId,
                            ServiceName = result.ServiceName,
                            Description = result.Description,
                            Image = result.Image != null
                                ? await _cloudStorageService.GetSignedUrlAsync(result.Image)
                                : null,
                            PriceType = result.PriceType,
                            Price = result.Price,
                            MinPrice = result.MinPrice,
                            Unit = result.Unit,
                            Rate = result.Rate,
                            Prices = servicePriceList,
                            TimeEstimate = result.TimeEstimate,
                            Rating = result.Rating,
                            NumOfRating = result.NumOfRating,
                        }
                    });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Manager, Staff")]
        //POST: api/orders
        [HttpPost("my-center/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestModel createOrderRequestModel)
        {
            try
            {
                var order = new Order();
                var customer = new Customer();
                if (ModelState.IsValid)
                {
                    var center = await _centerService.GetByIdToCreateOrder(createOrderRequestModel.CenterId);
                    DateTime UserPreferredDropoffTime;
                    if (!string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDropoffTime) &&
                        DateTime.TryParseExact(createOrderRequestModel.Order.PreferredDropoffTime,
                            "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out UserPreferredDropoffTime))
                    {
                        try
                        {
                            UserPreferredDropoffTime = DateTime.ParseExact(
                                createOrderRequestModel.Order.PreferredDropoffTime, "dd-MM-yyyy HH:mm:ss",
                                CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "PreferredDropoffTime: " + ex.Message,
                                Data = null
                            });
                            //Console.WriteLine("Failed to parse date: " + ex.Message);
                            // handle the parse failure
                        }
                    }
                    else
                    {
                        UserPreferredDropoffTime = DateTime.Now;
                    }

                    if (center == null)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "CenterId is not valid",
                            Data = null
                        });
                    }
                    else
                    {
                        var openTimePreferredDropoffDay = center.OperatingHours.FirstOrDefault(day =>
                            day.DaysOfWeekId == (int)UserPreferredDropoffTime.DayOfWeek);
                        //kiểm tra ngày giờ lấy hàng không là giờ hoạt động
                        if (openTimePreferredDropoffDay == null ||
                            openTimePreferredDropoffDay.OpenTime > UserPreferredDropoffTime.TimeOfDay ||
                            openTimePreferredDropoffDay.CloseTime < UserPreferredDropoffTime.TimeOfDay)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Center is closed at time that you choosen.",
                                Data = null
                            });
                        }
                    }

                    /*var promotion = new Promotion();
                    if (createOrderRequestModel.PromoCode != null)
                    {
                        var promo = await _promotionService.CheckValidPromoCode(createOrderRequestModel.CenterId, createOrderRequestModel.PromoCode);

                        if (promo == null)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "PromoCode is not valid",
                                Data = null
                            });
                        }

                        promotion = promo;
                    }
                    else
                    {
                        promotion = null;
                    }*/
                    //location
                    decimal? Latitude = null;
                    decimal? Longitude = null;
                    var ward = await _wardService.GetWardById(createOrderRequestModel.Order.CustomerWardId);
                    string AddressString = createOrderRequestModel.Order.CustomerAddressString;
                    string fullAddress = AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName +
                                         ", TP. Hồ Chí Minh";
                    string wardAddress = ward.WardName + ", " + ward.District.DistrictName + ", TP. Hồ Chí Minh";
                    var result = await SearchRelativeAddress(fullAddress);
                    if (result != null)
                    {
                        Latitude = result.lat;
                        Longitude = result.lon;
                    }
                    else
                    {
                        result = await SearchRelativeAddress(wardAddress);
                        if (result != null)
                        {
                            Latitude = result.lat;
                            Longitude = result.lon;
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

                    //add Location
                    var location = new Model.Models.Location();
                    location.AddressString = AddressString;
                    location.WardId = createOrderRequestModel.Order.CustomerWardId;
                    location.Latitude = Latitude;
                    location.Longitude = Longitude;
                    var locationResult = await _locationService.Add(location);

                    var customerByPhone =
                        await _customerService.GetByPhone(createOrderRequestModel.Order.CustomerMobile);

                    if (customerByPhone == null)
                    {
                        //add Customer
                        customer.Fullname = createOrderRequestModel.Order.CustomerName;
                        customer.Phone = createOrderRequestModel.Order.CustomerMobile;
                        customer.Address = locationResult.Id;
                        customer.Email = createOrderRequestModel.Order.CustomerEmail;
                        customer.Status = true;
                        customer.AccountId = null;
                        customer.CreatedDate = DateTime.Now;
                        customer.CreatedBy = "AutoInsert";
                        await _customerService.Add(customer);
                    }
                    else if (customerByPhone != null)
                    {
                        customer = customerByPhone;
                    }

                    var orders = await _orderService.GetAllOfDay(DateTime.Now.ToString("yyyyMMdd"));

                    int lastId = 0;
                    if (orders != null)
                    {
                        //var lastOrder = orders.LastOrDefault();
                        lastId = int.Parse(orders.Id.Substring(10));
                    }

                    //add Order
                    int newId = lastId + 1;
                    order.Id = $"{DateTime.Now.ToString("yyyyMMdd")}_{newId.ToString("D7")}";
                    order.CustomerName = createOrderRequestModel.Order.CustomerName;
                    order.LocationId = locationResult.Id;
                    order.CustomerEmail = createOrderRequestModel.Order.CustomerEmail;
                    order.CustomerMobile = createOrderRequestModel.Order.CustomerMobile;
                    order.CustomerMessage = createOrderRequestModel.Order.CustomerMessage;
                    order.CustomerId = customer.Id;
                    order.DeliveryType = createOrderRequestModel.Order.DeliveryType;
                    if (createOrderRequestModel.Order.DeliveryType == 0)
                    {
                        order.DeliveryPrice = null;
                    }

                    if (createOrderRequestModel.Order.DeliveryPrice != null)
                    {
                        order.DeliveryPrice = createOrderRequestModel.Order.DeliveryPrice;
                    }
                    else
                    {
                        order.DeliveryPrice = null;
                    }

                    order.PreferredDropoffTime =
                        string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDropoffTime)
                            ? null
                            : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDropoffTime,
                                "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    //order.PreferredDeliverTime = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDeliverTime) ? null : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDeliverTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    order.Status = "Confirmed";
                    order.CreatedBy = User.FindFirst(ClaimTypes.Role)?.Value;
                    order.CreatedDate = DateTime.Now;

                    //create List OrderDetails
                    var orderDetails = new List<OrderDetail>();
                    List<OrderDetailRequestModel> orderDetailRequestModels =
                        JsonConvert.DeserializeObject<List<OrderDetailRequestModel>>(createOrderRequestModel
                            .OrderDetails.ToJson());
                    decimal totalPayment = 0;
                    foreach (var item in orderDetailRequestModels)
                    {
                        var serviceItem = await _serviceService.GetById(item.ServiceId);
                        if (serviceItem == null && (!serviceItem.Status.ToLower().Equals("Updating") ||
                                                    !!serviceItem.Status.ToLower().Equals("Active")))
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Service is not valid",
                                Data = null
                            });
                        }
                        else
                        {
                            if (serviceItem.CenterId != createOrderRequestModel.CenterId)
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Service is not of this center of is not valid",
                                    Data = null
                                });
                            }
                        }

                        decimal currentPrice = 0;
                        decimal totalCurrentPrice = 0;
                        if (serviceItem.PriceType)
                        {
                            var priceChart = serviceItem.ServicePrices.OrderBy(a => a.MaxValue).ToList();
                            bool check = false;
                            foreach (var itemSerivePrice in priceChart)
                            {
                                if (item.Measurement <= itemSerivePrice.MaxValue && !check)
                                {
                                    currentPrice = itemSerivePrice.Price;
                                }

                                if (currentPrice > 0)
                                {
                                    check = true;
                                }
                            }

                            if (currentPrice * item.Measurement < serviceItem.MinPrice)
                            {
                                totalCurrentPrice = (decimal)serviceItem.MinPrice;
                            }
                            else
                            {
                                totalCurrentPrice = currentPrice * item.Measurement;
                            }
                        }
                        else
                        {
                            totalCurrentPrice = (decimal)serviceItem.Price * item.Measurement;
                        }

                        if (totalCurrentPrice != item.Price)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Service price with measurement has been changed.",
                                Data = null
                            });
                        }

                        orderDetails.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            ServiceId = item.ServiceId,
                            Measurement = item.Measurement,
                            Price = item.Price,
                            CustomerNote = item.CustomerNote,
                            StaffNote = User.FindFirst(ClaimTypes.Role)?.Value == "Staff" ? item.StaffNote : null,
                        });

                        totalPayment = totalPayment + item.Price;
                    }

                    //create List Deliveries

                    var deliveries = new List<Delivery>();
                    List<DeliveryRequestModel> deliveryRequestModels =
                        JsonConvert.DeserializeObject<List<DeliveryRequestModel>>(createOrderRequestModel.Deliveries
                            .ToJson());

                    foreach (var item in deliveryRequestModels)
                    {
                        //location
                        decimal? deliveryLatitude = null;
                        decimal? deliveryLongitude = null;
                        var deliveryWard = await _wardService.GetWardById(item.WardId);
                        string deliveryAddressString = item.AddressString;
                        string fullDeliveryAddress = deliveryAddressString + ", " + deliveryWard.WardName + ", " +
                                                     deliveryWard.District.DistrictName + ", TP. Hồ Chí Minh";
                        string wardDeliveryAddress = deliveryWard.WardName + ", " + deliveryWard.District.DistrictName +
                                                     ", TP. Hồ Chí Minh";
                        var resultDelivery = await SearchRelativeAddress(fullDeliveryAddress);
                        if (resultDelivery != null)
                        {
                            deliveryLatitude = resultDelivery.lat;
                            deliveryLongitude = resultDelivery.lon;
                        }
                        else
                        {
                            resultDelivery = await SearchRelativeAddress(wardDeliveryAddress);
                            if (resultDelivery != null)
                            {
                                deliveryLatitude = resultDelivery.lat;
                                deliveryLongitude = resultDelivery.lon;
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

                        //add Location
                        var deliveryLocation = new Model.Models.Location();
                        deliveryLocation.AddressString = deliveryAddressString;
                        deliveryLocation.WardId = item.WardId;
                        deliveryLocation.Latitude = deliveryLatitude;
                        deliveryLocation.Longitude = deliveryLongitude;
                        var deliveryLocationResult = await _locationService.Add(deliveryLocation);
                        var estimatedTime = await Utilities.CalculateDeliveryEstimatedTime(
                            Math.Round((decimal)deliveryLatitude, 6), Math.Round((decimal)deliveryLongitude, 6),
                            Math.Round((decimal)center.Location.Latitude, 6),
                            Math.Round((decimal)center.Location.Longitude, 6));
                        DateTime? deliveryDate = null;
                        // Dropoff = false, Deliver = true
                        if (!item.DeliveryType)
                        {
                            deliveryDate = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDropoffTime)
                                ? null
                                : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDropoffTime,
                                    "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                            if (deliveryDate == null)
                            {
                                deliveryDate = DateTime.Now;
                            }
                        }

                        deliveries.Add(new Delivery
                        {
                            OrderId = order.Id,
                            ShipperName = null,
                            ShipperPhone = null,
                            LocationId = deliveryLocationResult.Id,
                            EstimatedTime = estimatedTime,
                            DeliveryType = item.DeliveryType,
                            DeliveryDate = deliveryDate,
                            Status = "Pending",
                            CreatedBy = customer.Email != null
                                ? customer.Email
                                : createOrderRequestModel.Order.CustomerEmail,
                            CreatedDate = DateTime.Now
                        });
                    }

                    decimal paymentDelivery = 0;
                    if (order.DeliveryPrice != null && order.DeliveryPrice != 0)
                    {
                        paymentDelivery = (decimal)order.DeliveryPrice;
                    }

                    //create Payment
                    var payment = new Payment();
                    payment.OrderId = order.Id;
                    payment.PlatformFee = Utilities.platformFee;
                    payment.Date = null;
                    payment.Status = "Pending";
                    //payment.PromoCode = createOrderRequestModel.PromoCode != null ? promotion.Id : null;
                    payment.PromoCode = null;
                    payment.PaymentMethod = createOrderRequestModel.PaymentMethod;
                    //payment.Discount = promotion != null ? promotion.Discount : 0;
                    payment.Discount = 0;
                    //payment.Total = payment.PromoCode != null ? (totalPayment * (1 - promotion.Discount) + paymentDelivery) : (totalPayment + paymentDelivery);
                    payment.Total = (totalPayment + paymentDelivery);
                    payment.CreatedDate = DateTime.Now;
                    payment.CreatedBy = "AutoInsert";

                    //List OrderTracking
                    var orderTrackings = new List<OrderTracking>();
                    orderTrackings.Add(new OrderTracking
                    {
                        OrderId = order.Id,
                        Status = "Pending",
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                    });
                    orderTrackings.Add(new OrderTracking
                    {
                        OrderId = order.Id,
                        Status = "Confirmed",
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                    });
                    //
                    var orderAdded = await _orderService.Create(order, orderDetails, deliveries, payment, orderTrackings);

                    //Update Promotion UseTimes
                    /*if (promotion != null)
                    {
                        promotion.UseTimes = promotion.UseTimes - 1;
                        await _promotionService.Update(promotion);
                    }
*/
                    //

                    //notification
                    Notification notification = new Notification();
                    NotificationAccount notificationAccount = new NotificationAccount();
                    notification.OrderId = orderAdded.Id;
                    notification.CreatedDate = DateTime.Now;
                    notification.Title = "Thông báo về đơn hàng:  " + orderAdded.Id;
                    notification.Content = "Đơn hàng " + orderAdded.Id + " đã được tạo và đang chờ trung tâm xác nhận.";
                    await _notificationService.Add(notification);
                    //await _messageHub.Clients.All.NotifyToUser("NotificationAdded");
                    if (customer.AccountId != null)
                    {
                        //var cusinfo = _customerService.GetById(orderAdded.CustomerId);
                        //var accId = cusinfo.Result.AccountId ?? 0;
                        notificationAccount.AccountId = (int)(customer.AccountId);
                        notificationAccount.NotificationId = notification.Id;
                        await _notificationAccountService.Add(notificationAccount);
                        //await _messageHub.Clients.User(id).ReceiveNotification(notification.Content);
                        //await _messageHub.Clients.User(id).SendNotification(notificationAccount.AccountId, "NotificationAdded");
                        //await _messageHub(notificationAccount.AccountId, "NotificationAdded");
                        await messageHub.Clients.All.SendAsync("CreateOrder", notification);
                    }

                    var staff = _staffService.GetAllByCenterId(createOrderRequestModel.CenterId);
                    if (staff != null)
                    {
                        foreach (var staffItem in staff)
                        {
                            notificationAccount.AccountId = staffItem.AccountId;
                            notificationAccount.NotificationId = notification.Id;
                            await _notificationAccountService.Add(notificationAccount);
                            //await _messageHub.Clients.User(staffItem.AccountId.ToString()).ReceiveNotification(notification.Content);
                            //await _messageHub.Clients.User(staffItem.AccountId.ToString()).SendNotification(notificationAccount.AccountId, "NotificationAdded");
                            await messageHub.Clients.All.SendAsync("CreateOrder", notification);
                        }
                    }

                    var sendEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (sendEmail == null)
                    {
                        sendEmail = createOrderRequestModel.Order.CustomerEmail;
                    }

                    string path = "./Templates_email/CreateOrder.txt";
                    string content = System.IO.File.ReadAllText(path);
                    content = content.Replace("{recipient}", customer.Fullname);

                    content = content.Replace("{orderId}", orderAdded.Id);
                    await _sendMailService.SendEmailAsync(sendEmail, "Tạo đơn hàng", content);

                    return Ok(new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = new
                        {
                            OrderId = orderAdded.Id
                        }
                    });
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
            string url =
                $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={query}&format=json&limit=1";
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
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// type: dropoff/deliver
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="type"></param>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        [Authorize(Roles = "Staff,Manager")]
        // GET: api/manager/my-center/orders/{orderId}/deliveries/{type}/assign
        [HttpPut("my-center/orders/{orderId}/deliveries/{type}/assign")]
        [Produces("application/json")]
        public async Task<IActionResult> AssignStaffToDelivery(string orderId, string type,
            [FromBody] DriverInformationRequestModel updateModel)
        {
            try
            {
                if (updateModel.ShipperName == null && updateModel.ShipperPhone == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Not update",
                        Data = null
                    });
                }

                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
                        Data = ""
                    });
                }
                else
                {
                    var order = await _orderService.GetOrderWithDeliveries(orderId);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = ""
                        });
                    }

                    if (order.Deliveries.Count == 0)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Do not exist any delivery",
                            Data = ""
                        });
                    }

                    var deliveryItem = new Delivery();
                    if (type.ToLower().Trim().Equals("dropoff"))
                    {
                        deliveryItem = order.Deliveries.FirstOrDefault(deliver => deliver.DeliveryType == false);
                    }
                    else if (type.ToLower().Trim().Equals("deliver"))
                    {
                        deliveryItem = order.Deliveries.FirstOrDefault(deliver => deliver.DeliveryType == true);
                    }
                    else
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "type of deliver not correct",
                            Data = null
                        });
                    }
                    if (deliveryItem == null)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Not found delivery information",
                            Data = null
                        });
                    }
                    deliveryItem.ShipperName = updateModel.ShipperName;
                    deliveryItem.ShipperPhone = updateModel.ShipperPhone;
                    deliveryItem.UpdatedDate = DateTime.Now;
                    deliveryItem.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    await _deliveryService.Update(deliveryItem);
/*
                    //notification
                    string id = User.FindFirst("Id")?.Value;
                    Notification notification = new Notification();
                    NotificationAccount notificationAccount = new NotificationAccount();
                    notification.OrderId = order.Id;
                    notification.CreatedDate = DateTime.Now;
                    notification.Title = "Thông báo về đơn hàng:  " + order.Id;
                    notification.Content = "Đơn hàng " + order.Id + " đã được cập nhật.";
                    await _notificationService.Add(notification);

                    if (order.Customer.AccountId != null)
                    {
                        //var cusinfo = _customerService.GetById(orderAdded.CustomerId);
                        //var accId = cusinfo.Result.AccountId ?? 0;
                        notificationAccount.AccountId = (int)order.Customer.AccountId;
                        notificationAccount.NotificationId = notification.Id;
                        await _notificationAccountService.Add(notificationAccount);
                    }
                    var staff = _staffService.GetAllByCenterId(center.Id);
                    if (staff != null)
                    {
                        foreach (var staffItem in staff)
                        {
                            notificationAccount.AccountId = staffItem.AccountId;
                            notificationAccount.NotificationId = notification.Id;
                            await _notificationAccountService.Add(notificationAccount);
                        }
                    }*/

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            deliveryId = deliveryItem.Id,
                            orderId = orderId
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

        /// <summary>
        /// Pending->Delivering->Completed
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Authorize(Roles = "Staff,Manager")]
        // GET: api/manager/my-center/orders/{orderId}/deliveries/{type}/assign
        [HttpPut("my-center/orders/{orderId}/deliveries/{type}/change-status")]
        [Produces("application/json")]
        public async Task<IActionResult> ChangeDeliverStatus(string orderId, string type)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
                        Data = ""
                    });
                }
                else
                {
                    var order = await _orderService.GetOrderById(orderId);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = ""
                        });
                    }

                    if (order.Deliveries.Count == 0)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Do not exist any delivery",
                            Data = ""
                        });
                    }

                    var deliveryItem = new Delivery();
                    if (type.ToLower().Trim().Equals("dropoff"))
                    {
                        deliveryItem = order.Deliveries.FirstOrDefault(deliver => deliver.DeliveryType == false);
                        if (deliveryItem.Status.Trim().ToLower().Equals("pending"))
                        {
                            deliveryItem.Status = "Delivering";
                        }
                        else if (deliveryItem.Status.Trim().ToLower().Equals("delivering"))
                        {
                            deliveryItem.Status = "Completed";
                        }
                        else
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Do not accept update status",
                                Data = null
                            });
                        }
                    }
                    else if (type.ToLower().Trim().Equals("deliver"))
                    {
                        deliveryItem = order.Deliveries.FirstOrDefault(deliver => deliver.DeliveryType == true);
                        if (deliveryItem.Status.Trim().ToLower().Equals("pending"))
                        {
                            if (!order.Status.Trim().ToLower().Equals("ready"))
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Order is not ready",
                                    Data = null
                                });
                            }
                            else if (order.Payments.LastOrDefault().PaymentMethod != 0 &&
                                     !order.Payments.LastOrDefault().Status.Trim().ToLower().Equals("paid"))
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Payment is not paid",
                                    Data = null
                                });
                            }
                            else if (order.Status.Trim().ToLower().Equals("ready") &&
                                     (order.Payments.LastOrDefault().PaymentMethod == 0 ||
                                      (order.Payments.LastOrDefault().PaymentMethod != 0 &&
                                       order.Payments.LastOrDefault().Status.Trim().ToLower().Equals("paid"))))
                            {
                                deliveryItem.Status = "Delivering";
                            }
                            else
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Do not accept update status",
                                    Data = null
                                });
                            }
                        }
                        else if (deliveryItem.Status.Trim().ToLower().Equals("delivering"))
                        {
                            deliveryItem.Status = "Completed";
                            deliveryItem.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                            var payment = order.Payments.LastOrDefault();
                            if (payment.PaymentMethod == 1)
                            {
                                var walletTransaction = payment.WalletTransactions.FirstOrDefault();
                                if (walletTransaction != null &&
                                    walletTransaction.Status.Trim().ToLower().Equals("paid"))
                                {
                                    walletTransaction.Status = "Received";
                                    walletTransaction.UpdateTimeStamp = DateTime.Now;
                                    await _walletTransactionService.Update(walletTransaction);
                                    var wallet = await _walletService.GetById(walletTransaction.ToWalletId);
                                    wallet.Balance = wallet.Balance + order.Payments.LastOrDefault().Total -
                                                     order.Payments.LastOrDefault().PlatformFee;
                                    wallet.UpdatedDate = DateTime.Now;
                                    wallet.UpdatedBy = "Payment_Order_" + orderId;
                                    await _walletService.Update(wallet);
                                }
                            }

                            payment.Status = "Received";
                            payment.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                            payment.UpdatedDate = DateTime.Now;
                            await _paymentService.Update(payment);

                            await _deliveryService.Update(deliveryItem);

                            order.Status = "Completed";
                            order.UpdatedDate = DateTime.Now;
                            order.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                            await _orderService.Update(order);
                        }
                        else
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Do not accept update status",
                                Data = null
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "type of deliver not correct",
                            Data = null
                        });
                    }

                    await _deliveryService.Update(deliveryItem);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            deliveryId = deliveryItem.Id,
                            orderId = orderId
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

        [HttpPut("my-center/deactivate")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeactivateCenter()
        {
            var center = await _centerService.GetByIdLightWeight(int.Parse(User.FindFirst("CenterManaged")?.Value));
            if (center == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found center",
                    Data = ""
                });
            }

            if (!center.Status.Trim().ToLower().Equals("active"))
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Do not accept deactivate",
                    Data = null
                });
            }

            await _centerService.DeactivateCenter(center.Id);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = center
            });
        }

        [HttpPut("my-center/activate")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ActivateCenter()
        {
            var center = await _centerService.GetByIdLightWeight(int.Parse(User.FindFirst("CenterManaged")?.Value));
            if (center == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found center",
                    Data = ""
                });
            }

            if (!center.Status.Trim().ToLower().Equals("inactive"))
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Do not accept activate",
                    Data = null
                });
            }

            if (center.LastDeactivate != null && center.LastDeactivate > DateTime.Now.AddHours(-3))
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Only accept activate after 3 hours from last deactivated",
                    Data = null
                });
            }

            await _centerService.ActivateCenter(center.Id);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = center
            });
        }

        [HttpGet("my-center/feedbacks")]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> GetMyCenterFeedback([FromQuery] GetCenterFeedbacksModel filter)
        {
            var center = await _centerService.GetByIdLightWeight(int.Parse(User.FindFirst("CenterManaged")?.Value));
            if (center == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found center",
                    Data = ""
                });
            }

            var feedbacks = _feedbackService.GetAllByCenterId(center.Id);
            if (feedbacks == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Center not have any feedback",
                    Data = null
                });
            }
            feedbacks = feedbacks.OrderByDescending(x => x.CreatedDate).ToList();
            if (filter.ServiceId != null)
            {
                feedbacks = feedbacks.Where(fb => Equals(fb.ServiceId, filter.ServiceId));
            }

            if (!string.IsNullOrEmpty(filter.OrderId))
            {
                feedbacks = feedbacks.Where(fb => Equals(fb.OrderId, filter.OrderId));
            }

            int totalItems = feedbacks.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);
            feedbacks = feedbacks.Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToList();


            var feedbackResponses = new List<FeedbackResponseModel>();
            foreach (var item in feedbacks)
            {
                string centerName, serviceName;
                if (item.CenterId != null)
                {
                    centerName = item.Center.CenterName;
                }
                else centerName = null;

                if (item.ServiceId != null)
                {
                    serviceName = item.Service.ServiceName;
                }
                else serviceName = null;
                var account = _accountService.GetAccountByEmail(item.CreatedBy);
                feedbackResponses.Add(new FeedbackResponseModel
                {
                    Id = item.Id,
                    Content = item.Content,
                    Rating = item.Rating,
                    OrderId = item.OrderId,
                    CenterId = item.CenterId,
                    CenterName = centerName,
                    ServiceId = item.ServiceId,
                    ServiceName = serviceName,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    ReplyMessage = item.ReplyMessage,
                    ReplyBy = item.ReplyBy,
                    ReplyDate = item.ReplyDate.HasValue
                        ? item.ReplyDate.Value.ToString("dd-MM-yyyy HH:mm:ss")
                        : null,
                    AccountAvatar = account.ProfilePic != null
                        ? await _cloudStorageService.GetSignedUrlAsync(account.ProfilePic)
                        : null
                    ,
                    AccountName = account.FullName
                });
            }


            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "success",
                Data = new
                {
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    ItemsPerPage = filter.PageSize,
                    PageNumber = filter.Page,
                    Items = feedbackResponses
                }
            });
        }


        [HttpGet("my-center/staff-statistics")]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> GetStaffStatistics()
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are manager",
                        Data = null
                    });
                }
                else
                {
                    var staffStatistic = await _orderService.GetStaffStatistics(center.Id);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = staffStatistic
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

        [HttpGet("my-center/services")]
        [Authorize(Roles = "Manager, Staff")]
        public async Task<IActionResult> GetCenterServices()
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetByIdLightWeight((int)managerInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center",
                        Data = null
                    });
                }
                var services = await _serviceService.GetAllByCenterId(center.Id);
                services = services.Where(ser => ser.Status.ToLower().Trim().Equals("active")).ToList();
                var servicesOfCenter = new List<MyCenterServiceResponseModel>();
                var centerServices = new List<ManagerCenterServiceResponseModel>();
                foreach (var item in services)
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

                    var service = new MyCenterServiceResponseModel
                    {
                        ServiceId = item.Id,
                        CategoryId = item.CategoryId,
                        ServiceName = item.ServiceName,
                        PriceType = item.PriceType,
                        Price = item.Price,
                        MinPrice = item.MinPrice,
                        Unit = item.Unit,
                        Rate = item.Rate,
                        Prices = servicePriceViewModels.OrderByDescending(a => a.Price).ToList(),
                        TimeEstimate = item.TimeEstimate
                    };
                    servicesOfCenter.Add(service);
                }
                decimal minPrice = 0, maxPrice = 0;
                foreach (var service in services)
                {
                    if (service.PriceType)
                    {
                        foreach (var servicePrice in service.ServicePrices)
                        {
                            if (minPrice > service.MinPrice || minPrice == 0)
                            {
                                minPrice = (decimal)service.MinPrice;
                            }

                            if (maxPrice < (servicePrice.Price * servicePrice.MaxValue) || maxPrice == 0)
                            {
                                maxPrice = (decimal)(servicePrice.Price * servicePrice.MaxValue);
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

                    var centerService = new ManagerCenterServiceResponseModel
                    {
                        ServiceCategoryID = service.CategoryId,
                        ServiceCategoryName = service.Category.CategoryName,
                        Services = servicesOfCenter.Where(ser => ser.CategoryId == service.CategoryId).ToList()
                    };
                    if (centerServices.FirstOrDefault(cs =>
                            cs.ServiceCategoryID == centerService.ServiceCategoryID) ==
                        null) centerServices.Add(centerService);
                }
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = centerServices
                });
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