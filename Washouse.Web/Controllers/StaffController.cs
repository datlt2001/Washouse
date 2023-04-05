using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Washouse.Model.ResponseModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using System.Linq;
using Washouse.Model.ViewModel;

namespace Washouse.Web.Controllers
{
    [Route("api/staffs")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        public readonly IStaffService _staffService;
        public readonly IAccountService _accountService;
        private readonly ICenterService _centerService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly ILocationService _locationService;
        private readonly IWardService _wardService;
        private readonly IOperatingHourService _operatingHourService;
        private readonly IServiceService _serviceService;
        private readonly ICenterRequestService _centerRequestService;
        private readonly IFeedbackService _feedbackService;
        private readonly IPromotionService _promotionService;

        public StaffController(IStaffService staffService, IAccountService accountService,
                                ICenterService centerService, ICloudStorageService cloudStorageService,
                                ILocationService locationService, IWardService wardService,
                                IOperatingHourService operatingHourService, IServiceService serviceService,
                                ICenterRequestService centerRequestService,
                                IFeedbackService feedbackService, IPromotionService promotionService)
        {
            this._staffService = staffService;
            this._accountService = accountService; 
            this._centerService = centerService;
            this._locationService = locationService;
            this._cloudStorageService = cloudStorageService;
            this._wardService = wardService;
            this._operatingHourService = operatingHourService;
            this._serviceService = serviceService;
            this._centerRequestService = centerRequestService;
            this._feedbackService = feedbackService;
            this._promotionService = promotionService;
        }

        [HttpGet]
        public IActionResult GetStaffList()
        {
            var staff = _staffService.GetAll();
            if (staff == null) { return NotFound(); }
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffrById(int id)
        {
            var staff = await _staffService.GetById(id);
            if (staff == null) { return NotFound(); }
            return Ok(staff);
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateStaff(int id)
        {
            var staff = await _staffService.GetById(id);
            if (staff == null)
            {
                return NotFound();
            }
            await   _staffService.DeactivateStaff(id);
            return Ok();
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateStaff(int id)
        {
            var staff = await _staffService.GetById(id);
            if (staff == null)
            {
                return NotFound();
            }
            await _staffService.ActivateStaff(id);
            return Ok();
        }

        [HttpPut("{staffId}")]
        public async Task<IActionResult> UpdateProfile([FromBody] StaffRequestModel input, int staffId)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                Staff existingStaff = await _staffService.GetById(staffId);
                var accountId = existingStaff.AccountId;
                //int userId = accountId ?? 0;
                Account user = await _accountService.GetById(accountId);

                if (existingStaff == null) { return NotFound(); }
                else
                {
                                     
                    existingStaff.UpdatedDate = DateTime.Now;
                    existingStaff.UpdatedBy = input.FullName;
                    //existingCustomer.Address = input.LocationId;
                    existingStaff.IdNumber = input.IdNumber;
                    //existingStaff.IdFrontImg = input.IdNumber;
                    //existingStaff.IdBackImg = input.IdNumber;

                    await _staffService.Update(existingStaff);

                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedBy = user.FullName;
                    user.FullName = input.FullName;
                    user.Dob = input.Dob;
                    user.Email = input.Email;
                    user.ProfilePic = input.SavedFileName;

                    await _accountService.Update(user);


                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Updated",
                        Data = existingStaff
                    });
                }


            }

            
        }

        // GET: api/staffs/center
        [HttpGet("center")]
        public async Task<IActionResult> GetCenterOfStaff()
        {
            try
            {
                var staffInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetById((int)staffInfo.CenterId);
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
                    //decimal minPrice = 0, maxPrice = 0;
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
                    //response.Status = center.Status;
                    //response.TaxCode = center.TaxCode.Trim();
                    //response.TaxRegistrationImage = center.TaxRegistrationImage;
                    response.MonthOff = MonthOff;
                    response.HasDelivery = center.HasDelivery;
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

        // GET: api/staffs/services
        [HttpGet("services")]
        public async Task<IActionResult> GetServicesOfCenterWoking()
        {
            try
            {
                var staffInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetById((int)staffInfo.CenterId);
                if (center == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found center that you are working",
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


    }
}
