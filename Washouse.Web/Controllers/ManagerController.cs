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
        public ManagerController(ICenterService centerService, ICloudStorageService cloudStorageService,
                                ILocationService locationService, IWardService wardService,
                                IOperatingHourService operatingHourService, IServiceService serviceService,
                                IStaffService staffService, ICenterRequestService centerRequestService, 
                                IFeedbackService feedbackService, IPromotionService promotionService,
                                ICustomerService customerService, IOrderService orderService)
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
        }

        #endregion

        // GET: api/manager/my-center
        [Authorize(Roles = "Manager,Staff")]
        [HttpGet("my-center")]
        public async Task<IActionResult> GetMyCenter()
        {
            try
            {
                var managerInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetById((int)managerInfo.CenterId);
                if (center == null) {
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
                    response.Thumbnail = center.Image != null ? await _cloudStorageService.GetSignedUrlAsync(center.Image) : null;
                    response.Title = center.CenterName;
                    response.Alias = center.Alias;
                    response.Description = center.Description;
                    response.Rating = center.Rating;
                    response.NumOfRating = center.NumOfRating;
                    response.Phone = center.Phone;
                    response.CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName + ", " + center.Location.Ward.District.DistrictName;
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
                            OrderId = item.OrderDetail.OrderId,
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
        public async Task<IActionResult> GetServicesOfCenterManaged()
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
                                if (feedback.Rating == 1) { st1++; }
                                if (feedback.Rating == 2) { st2++; }
                                if (feedback.Rating == 3) { st3++; }
                                if (feedback.Rating == 4) { st4++; }
                                if (feedback.Rating == 5) { st5++; }
                            }
                            var itemResponse = new ServiceCenterModel
                            {
                                ServiceId = item.Id,
                                ServiceName = item.ServiceName,
                                Alias = item.Alias,
                                CategoryId = item.CategoryId,
                                Description = item.Description,
                                Image = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
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
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = servicesOfCenter
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
                    if (promotion == null) {
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
                                _startDate = item.StartDate.Value.ToString("dd-MM-yyyy HH-mm-ss");
                            }
                            if (item.ExpireDate.HasValue)
                            {
                                _expireDate = item.ExpireDate.Value.ToString("dd-MM-yyyy HH-mm-ss");
                            }
                            if (item.UpdatedDate.HasValue)
                            {
                                _updatedDate = item.UpdatedDate.Value.ToString("dd-MM-yyyy HH-mm-ss");
                            }

                            var itemResponse = new PromotionCenterModel
                            {
                                Code = item.Code,
                                Description = item.Description,
                                Discount = item.Discount,
                                StartDate = _startDate,
                                ExpireDate = _expireDate,
                                CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH-mm-ss"),
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
        public async Task<IActionResult> GetCustomerOfCenterManaged()
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
                        var customers = new List<CustomerCenterModel>();
                        foreach (var item in customersOfCenter)
                        {
                            string addressStringResponse = null;
                            if (item.Address != null)
                            {
                                var location = await _locationService.GetById(item.Address.Value);
                                addressStringResponse = location.AddressString + ", " + location.Ward.WardName + ", " + location.Ward.District.DistrictName + ", " + "Thành Phố Hồ Chí Minh";
                            }
                            var itemResponse = new CustomerCenterModel
                            {
                                Id = item.Id,
                                AccountId = item.AccountId,
                                Fullname = item.Fullname,
                                Phone = item.Phone,
                                Email = item.Email,
                                AddressString = addressStringResponse
                            };
                            customers.Add(itemResponse);
                        }
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "success",
                            Data = customers
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
        // GET: api/manager/my-center/orders
        [HttpGet("my-center/orders")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetOrdersOfCenter([FromQuery] FilterOrdersRequestModel filterOrdersRequestModel)
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
                } else
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
                        orders = orders.Where(order => (order.OrderDetails.Any(orderDetail => (orderDetail.Service.ServiceName.ToLower().Contains(filterOrdersRequestModel.SearchString.ToLower())
                                                                                    || (orderDetail.Service.Alias != null && orderDetail.Service.Alias.ToLower().Contains(filterOrdersRequestModel.SearchString.ToLower()))))
                                                           || order.Id.ToLower().Contains(filterOrdersRequestModel.SearchString.ToLower())
                                                           || order.OrderDetails.FirstOrDefault().Service.Center.CenterName.ToLower().Contains(filterOrdersRequestModel.SearchString.ToLower())))
                                              .ToList();

                    }
                    var response = new List<OrderCenterModel>();
                    foreach (var order in orders)
                    {
                        decimal TotalOrderValue = 0;
                        foreach (var item in order.OrderDetails)
                        {
                            TotalOrderValue += item.Price;
                        }
                        string _orderDate = null;
                        if (order.CreatedDate.HasValue)
                        {
                            _orderDate = order.CreatedDate.Value.ToString("dd-MM-yyyy HH-mm-ss");
                        }
                        response.Add(new OrderCenterModel
                        {
                            OrderId = order.Id,
                            OrderDate = _orderDate,
                            CustomerName = order.CustomerName,
                            TotalOrderValue = TotalOrderValue,
                            Discount = order.Payments.Count > 0 ? order.Payments.First().Discount : 0,
                            TotalOrderPayment = order.Payments.Count > 0 ? order.Payments.First().Total : 0,
                            Status = order.Status
                        });
                    }
                    int totalItems = response.Count();
                    int totalPages = (int)Math.Ceiling((double)totalItems / filterOrdersRequestModel.PageSize);

                    response = response.Skip((filterOrdersRequestModel.Page - 1) * filterOrdersRequestModel.PageSize).Take(filterOrdersRequestModel.PageSize).ToList();
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

    }
}
