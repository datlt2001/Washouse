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
using Twilio.Http;
using static Google.Apis.Requests.BatchRequest;
using Washouse.Model.RequestModels;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using Washouse.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

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
        private readonly IHubContext<MessageHub> messageHub;

        public ManagerController(ICenterService centerService, ICloudStorageService cloudStorageService,
            ILocationService locationService, IWardService wardService,
            IOperatingHourService operatingHourService, IServiceService serviceService,
            IStaffService staffService, ICenterRequestService centerRequestService,
            IFeedbackService feedbackService, IPromotionService promotionService,
            INotificationService notificationService, INotificationAccountService notificationAccountService,
            ICustomerService customerService, IOrderService orderService, IAccountService accountService,
            IHubContext<MessageHub> _messageHub)
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
            messageHub = _messageHub;
        }

        #endregion

        [Authorize(Roles = "Manager")]
        [HttpPut("my-center")]
        public async Task<IActionResult> UpdateMyCenter(CenterEditRequestModel centerEditRequest)
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

                var center = await _centerService.GetById((int)managerInfo.CenterId);

                var centerRequesting = await _centerService.GetById(center.Id);
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
                    var ward = new Ward();
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
                                         ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
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
                    }

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
                    centerRequestModel.HotFlag = centerRequesting.HotFlag;
                    centerRequestModel.Rating = centerRequesting.Rating;
                    centerRequestModel.NumOfRating = centerRequesting.NumOfRating;
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

                var center = await _centerService.GetById((int)managerInfo.CenterId);
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
                    foreach (var item in center.Feedbacks)
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

                    response.CenterFeedbacks = centerFeedbacks;
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
                var center = await _centerService.GetById((int)managerInfo.CenterId);
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
                    var services = center.Services.ToList();
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
                                    Items = servicesOfCenter
                                }
                            });
                        }
                        else if (filter.Pagination == null)
                        {
                            filter.Pagination = new PaginationViewModel { Page = 1, PageSize = 10 };
                        }

                        int totalPages = (int)Math.Ceiling((double)totalItems / filter.Pagination.PageSize);
                        servicesOfCenter = servicesOfCenter
                            .Skip((filter.Pagination.Page - 1) * filter.Pagination.PageSize)
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
        // GET: api/manager/promotions
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotionsOfCenterManaged()
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetById((int)managerInfo.CenterId);
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
                    var promotion = _promotionService.GetAll();
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
                                Code = item.Code,
                                Description = item.Description,
                                Discount = item.Discount,
                                StartDate = _startDate,
                                ExpireDate = _expireDate,
                                CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                                UpdatedDate = _updatedDate,
                                UseTimes = item.UseTimes
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

        [Authorize(Roles = "Manager,Staff")]
        // GET: api/manager/my-center/customers
        [HttpGet("my-center/customers")]
        public async Task<IActionResult> GetCustomerOfCenterManaged(
            [FromQuery] FilterCustomersOfCenterRequestModel filter)
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetById((int)managerInfo.CenterId);
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
                                var location = await _locationService.GetById(item.Address.Value);
                                addressStringResponse = location.AddressString + ", " + location.Ward.WardName + ", " +
                                                        location.Ward.District.DistrictName + ", " +
                                                        "Thành Phố Hồ Chí Minh";
                            }

                            string dob = null;
                            int? gender = null;
                            if (item.AccountId != null)
                            {
                                var account = await _accountService.GetById(item.AccountId.Value);
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
                var center = await _centerService.GetById((int)managerInfo.CenterId);
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
                    if (orders.Count() == 0)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found orders of center.",
                            Data = null
                        });
                    }

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

                    if (filterOrdersRequestModel.DeliveryType != null)
                    {
                        orders = orders.Where(
                            order => Equals(order.DeliveryType, filterOrdersRequestModel.DeliveryType));
                    }

                    if (filterOrdersRequestModel.DeliveryStatus != null)
                    {
                        orders = orders.Where(order => order.Deliveries.Any(delivery =>
                            Equals(delivery.Status.ToLower(), filterOrdersRequestModel.DeliveryStatus.ToLower())));
                    }

                    var response = new List<OrderCenterModel>();

                    orders = orders.OrderByDescending(x => x.CreatedDate).ToList();
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
                            Deliveries = order.Deliveries.ToList().ConvertAll(delivery => new OrderedDeliveryModel
                            {
                                DeliveryStatus = delivery.Status
                            }),
                            OrderedServices = orderedServices
                        });
                    }

                    int totalItems = response.Count();
                    if (filterOrdersRequestModel.PageSize == -1)
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
                                Items = response
                            }
                        });
                    }

                    int totalPages = (int)Math.Ceiling((double)totalItems / filterOrdersRequestModel.PageSize);
                    response = response.Skip((filterOrdersRequestModel.Page - 1) * filterOrdersRequestModel.PageSize)
                        .Take(filterOrdersRequestModel.PageSize).ToList();
                    if (response.Count > 0)
                    {
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
                var center = await _centerService.GetById((int)managerInfo.CenterId);
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
                    var order = await _orderService.GetOrderById(id);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = null
                        });
                    }

                    var response = new OrderInfomationModel();
                    response.Id = id;
                    response.CustomerName = order.CustomerName;
                    response.LocationId = order.LocationId;
                    response.CustomerEmail = order.CustomerEmail;
                    response.CustomerMobile = order.CustomerMobile;
                    response.CustomerMessage = order.CustomerMessage;
                    response.CustomerOrdered = order.CustomerId;
                    response.DeliveryType = order.DeliveryType;
                    response.DeliveryPrice = order.DeliveryPrice;
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
                            Price = item.Service.Price,
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
                        OrderDeliveries.Add(new OrderDeliveryModel
                        {
                            ShipperName = delivery.ShipperName,
                            ShipperPhone = delivery.ShipperPhone,
                            LocationId = delivery.LocationId,
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
                var center = await _centerService.GetById((int)managerInfo.CenterId);
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

                    if (orderDetail.Status.Trim().ToLower().Equals("completed"))
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Order detail has been already completed",
                            Data = null
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
                            decimal total = payment.Total / (1 - (decimal)payment.Discount) - orderDetail.Price +
                                            totalCurrentPrice;
                            if (total > 0)
                            {
                                payment.Total = total;
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
    }
}