using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/centers")]
    [ApiController]
    public class CenterController : ControllerBase
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
        public CenterController(ICenterService centerService, ICloudStorageService cloudStorageService,
                                ILocationService locationService, IWardService wardService,
                                IOperatingHourService operatingHourService, IServiceService serviceService,
                                IStaffService staffService, ICenterRequestService centerRequestService)
        {
            this._centerService = centerService; 
            this._locationService = locationService; 
            this._cloudStorageService = cloudStorageService;
            this._wardService = wardService;
            this._operatingHourService = operatingHourService;
            this._serviceService = serviceService;
            this._staffService = staffService;
            this._centerRequestService = centerRequestService;
        }

        #endregion

        /// <summary>
        /// Gets the list of all Centers.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/centers
        ///     {        
        ///       "page": 1,
        ///       "pageSize": 5,
        ///       "sort": "location",
        ///       "budgetRange": "30000-50000",
        ///       "categoryServices": "2,3",
        ///       "searchString": "Dr"  
        ///       "currentUserLatitude": 10.6
        ///       "currentUserLongitude": 106.8
        ///     }
        /// </remarks>
        /// <returns>The list of Centers.</returns>
        /// <response code="200">Success return list ceters</response>   
        /// <response code="404">Not found any center matched</response>   
        /// <response code="400">One or more error occurs</response>   
        // GET: api/centers
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll([FromQuery] FilterCentersRequestModel filterCentersRequestModel)
        {
            try
            {
                var centerList = await _centerService.GetAll();
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null || role.Trim().ToLower().Equals("customer"))
                {
                    centerList = centerList.Where(center => center.Status.Trim().ToLower().Equals("active")
                                                        || center.Status.Trim().ToLower().Equals("updatepending"));
                }
                if (filterCentersRequestModel.SearchString != null)
                {
                    centerList = centerList.Where(res => res.CenterName.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower())
                                                  || (res.Alias != null && res.Alias.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower()))
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
                    double distance = 0;
                    if (center.Location.Latitude == null || center.Location.Longitude == null || filterCentersRequestModel.CurrentUserLatitude == null || filterCentersRequestModel.CurrentUserLongitude == null)
                    {
                        distance = 0;
                    } else
                    {
                        distance = Utilities.CalculateDistance(Math.Round((decimal)filterCentersRequestModel.CurrentUserLatitude, 6), Math.Round((decimal)filterCentersRequestModel.CurrentUserLongitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
                    }
                    if (center.MonthOff != null)
                    {
                        string[] offs = center.MonthOff.Split('-');
                        for (int i = 0; i < offs.Length; i++)
                        {
                            if (DateTime.Now.Day == (int.Parse(offs[i]))) {
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
                        if (item.ToLower().Equals("rating"))
                        {
                            response = response.OrderByDescending(res => res.Rating).ThenBy(res => res.Distance).ToList();
                        } else if (item.ToLower().Equals("location"))
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
                        Data = new {
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            ItemsPerPage = filterCentersRequestModel.PageSize,
                            PageNumber = filterCentersRequestModel.Page,
                            Items = response
                        }
                    });
                } else
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

        //[Route("Details/{id}")]
        // GET: api/centers/2
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, decimal? CurrentUserLatitude, decimal? CurrentUserLongitude)
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
                            Image  = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
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
                    double distance = 0;
                    if (center.Location.Latitude == null || center.Location.Longitude == null || CurrentUserLatitude == null || CurrentUserLongitude == null)
                    {
                        distance = 0;
                    }
                    else
                    {
                        distance = Utilities.CalculateDistance(Math.Round((decimal)CurrentUserLatitude, 6), Math.Round((decimal)CurrentUserLongitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
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
                    response.Distance = distance;
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

        /// <summary>
        /// Create a center.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/centers
        ///     { 
        ///       "center": 
        ///       {
        ///         "centerName": "Giặt sấy Quỳnh Phát",
        ///         "alias": "Giặt sấy Quỳnh Phát",
        ///         "phone": "0988714029",
        ///         "description": "Giặt nhanh, sạch, thơm",
        ///         "monthOff": "16",
        ///         "savedFileName": "ThumbnailCenter01-20230322184337.png"
        ///       },
        ///       location: {
        ///         "addressString": "103 Quang Trung",
        ///         "wardId": 40,
        ///         "latitude": 0,
        ///         "longitude": 0
        ///       },
        ///       "centerOperatingHours": [
        ///         {
        ///             "day": 0,
        ///             "openTime": "06:00",
        ///             "closeTime": "21:00"
        ///         },
        ///         {
        ///             "day": 1,
        ///             "openTime": "06:00",
        ///             "closeTime": "21:00"
        ///         },
        ///         {
        ///             "day": 2,
        ///             "openTime": "06:00",
        ///             "closeTime": "21:00"
        ///         },
        ///         {
        ///             "day": 3,
        ///             "openTime": "06:00",
        ///             "closeTime": "21:00"
        ///         },
        ///         {
        ///             "day": 4,
        ///             "openTime": "06:00",
        ///             "closeTime": "21:00"
        ///         },
        ///         {
        ///             "day": 5,
        ///             "openTime": "06:00",
        ///             "closeTime": "21:00"
        ///         },
        ///         {
        ///             "day": 6,
        ///             "openTime": "06:00",
        ///             "closeTime": "12:00"
        ///         }
        ///       ],
        ///       "resources": [
        ///         {
        ///             "name": "Máy giặt cửa ngang 10kg UltimateCare 300",
        ///             "alias": "ewf1024d3wb",
        ///             "quantity": 4,
        ///             "washCapacity": 10,
        ///             "dryCapacity": 0,
        ///             "availableQuantity": 4
        ///         },
        ///         {
        ///             "name": "Máy giặt sấy 11/7kg UltimateCare 700",
        ///             "alias": "EWW1142Q7WB",
        ///             "quantity": 6,
        ///             "washCapacity": 11,
        ///             "dryCapacity": 7,
        ///             "availableQuantity": 4
        ///         }
        ///       ]
        ///     }
        ///   
        /// </remarks>
        /// 
        /// <returns>Center created.</returns>
        /// <response code="200">Success create a ceter</response>     
        /// <response code="400">One or more error occurs</response>   
        // POST: api/centers
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCenter([FromBody] CreateCenterRequestModel createCenterRequestModel)
        {
            try
            {
                Center center = new Center();
                var location = new Model.Models.Location();
                if (ModelState.IsValid)
                {
                    //Add Location
                    location.AddressString = createCenterRequestModel.Location.AddressString;
                    location.WardId = createCenterRequestModel.Location.WardId;
                    var ward = new Ward();
                    try
                    {
                        ward = await _wardService.GetWardById(location.WardId);
                    } catch
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found ward by wardId.",
                            Data = null
                        });
                    }
                    string fullAddress = createCenterRequestModel.Location.AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                    string url = $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={fullAddress}&format=json&limit=1";
                    using (HttpClient client = new HttpClient())
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
                    if (createCenterRequestModel.Location.Latitude != null && createCenterRequestModel.Location.Latitude != 0)
                    {
                        location.Latitude = createCenterRequestModel.Location.Latitude;
                    }
                    if (createCenterRequestModel.Location.Longitude != null && createCenterRequestModel.Location.Longitude != 0)
                    {
                        location.Longitude = createCenterRequestModel.Location.Longitude;
                    }
                    if (location.Latitude != null && location.Longitude != null && location.Latitude != 0 && location.Longitude != 0)
                    {
                        location.Latitude = Math.Round((decimal)location.Latitude,9);
                        location.Longitude = Math.Round((decimal)location.Longitude, 9);
                    } else
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Location of center(latitude and longitude) not recognized or not in Ho Chi Minh city.",
                            Data = null
                        });
                    }
                    var locationAdded = await _locationService.Add(location);

                    var checkLocationExistCenter = await _locationService.GetById(locationAdded.Id);
                    if (checkLocationExistCenter.Centers.ToList() != null) 
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
                    //Add Center 
                    /*if (createCenterRequestModel.Center.HasDelivery == null)
                    {
                        createCenterRequestModel.Center.HasDelivery = false;
                    }*/
                    center.Id = 0;
                    center.CenterName = createCenterRequestModel.Center.CenterName;
                    center.Alias = createCenterRequestModel.Center.Alias;
                    center.LocationId = locationAdded.Id;
                    center.Phone = createCenterRequestModel.Center.Phone;
                    center.Description = createCenterRequestModel.Center.Description;
                    center.MonthOff = createCenterRequestModel.Center.MonthOff;
                    center.IsAvailable = false;
                    center.Status = "CreatePending";
                    center.Image = createCenterRequestModel.Center.SavedFileName;
                    center.TaxCode = createCenterRequestModel.Center.TaxCode;
                    center.TaxRegistrationImage = createCenterRequestModel.Center.TaxRegistrationImage;
                    center.HotFlag = false;
                    center.Rating = null;
                    center.NumOfRating = 0;
                    //center.HasDelivery = (bool)createCenterRequestModel.Center.HasDelivery;
                    center.HasDelivery = false;
                    center.CreatedDate = DateTime.Now;
                    center.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    center.UpdatedDate = null;
                    center.UpdatedBy = null;

                    await _centerService.Add(center);

                    //Add Operating time
                    List<OperatingHoursRequestModel> operatings = JsonConvert.DeserializeObject<List<OperatingHoursRequestModel>>(createCenterRequestModel.CenterOperatingHours.ToJson());
                    foreach (var item in operatings)
                    {
                        var operatingTime = new OperatingHour();
                        operatingTime.CenterId = center.Id;
                        operatingTime.DaysOfWeekId = item.Day;
                        var openTime = item.OpenTime.Split(':');
                        operatingTime.OpenTime = new TimeSpan(int.Parse(openTime[0]), int.Parse(openTime[1]), 0);
                        var closeTime = item.CloseTime.Split(':');
                        operatingTime.CloseTime = new TimeSpan(int.Parse(closeTime[0]), int.Parse(closeTime[1]), 0);
                        operatingTime.CreatedDate = DateTime.Now;
                        operatingTime.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                        operatingTime.UpdatedDate = null;
                        operatingTime.UpdatedBy = null;

                        await _operatingHourService.Add(operatingTime);
                    }

                    //Add Resources
                    /*List<ResourceRequestModel> resources = JsonConvert.DeserializeObject<List<ResourceRequestModel>>(createCenterRequestModel.Resources.ToJson());
                    foreach (var item in resources)
                    {
                        var resource = new Resourse();
                        resource.CenterId = center.Id;
                        resource.ResourceName = item.Name;
                        resource.Alias = item.Alias;
                        resource.Quantity = item.Quantity;
                        resource.AvailableQuantity = item.AvailableQuantity;
                        resource.WashCapacity = item.WashCapacity;
                        resource.DryCapacity = item.DryCapacity;
                        resource.Status = true;
                        resource.CreatedDate = DateTime.Now;
                        resource.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                        resource.UpdatedDate = null;
                        resource.UpdatedBy = null;

                    }*/

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new { CenterId = center.Id }
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

        [HttpGet("{id}/services")]
        public async Task<IActionResult> GetServicesOfACenter(int id)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    var services = center.Services.ToList();
                    if (services != null)
                    {
                        var servicesOfCenter = new List<ServicesOfCenterResponseModel>();
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
                            var itemResponse = new ServicesOfCenterResponseModel
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


        [HttpGet("{centerId}/staffs")]
        public IActionResult GetStaffByCenterId(int centerId)
        {
            var staff = _staffService.GetAllByCenterId(centerId);
            if (staff == null) return NotFound();
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = staff
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, CenterEditRequestModel center)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var centerRequesting = await _centerService.GetById(id);
                    if (centerRequesting == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found center",
                            Data = null
                        });
                    }

                    if (id != centerRequesting.Id)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Error occurs",
                            Data = null
                        });
                    }
                    //"location": {
                    //"addressString": "103 Quang Trung",
                    //"wardId": 40,
                    // "latitude": 0,
                    //"longitude": 0 }
                    //Add Location

                    var location = new Model.Models.Location();
                    var checkLocationExistCenter = new Model.Models.Location();
                    if (center.Location != null)
                    {

                        location.AddressString = center.Location.AddressString;
                        location.WardId = center.Location.WardId;
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
                        string fullAddress = center.Location.AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                        string url = $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={fullAddress}&format=json&limit=1";
                        using (HttpClient client = new HttpClient())
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
                        if (center.Location.Latitude != null && center.Location.Latitude != 0)
                        {
                            location.Latitude = center.Location.Latitude;
                        }
                        if (center.Location.Longitude != null && center.Location.Longitude != 0)
                        {
                            location.Longitude = center.Location.Longitude;
                        }
                        if (location.Latitude != null && location.Longitude != null && location.Latitude != 0 && location.Longitude != 0)
                        {
                            location.Latitude = Math.Round((decimal)location.Latitude, 9);
                            location.Longitude = Math.Round((decimal)location.Longitude, 9);
                        }
                        else
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Location of center(latitude and longitude) not recognized or not in Ho Chi Minh city.",
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
                    if (center != null)
                    {
                        centerRequestModel.CenterRequesting = centerRequesting.Id;
                        centerRequestModel.RequestStatus = true;
                        centerRequestModel.CenterName = string.IsNullOrWhiteSpace(center.CenterName) ? centerRequesting.CenterName : center.CenterName.Trim();
                        centerRequestModel.Alias = string.IsNullOrWhiteSpace(center.Alias) ? centerRequesting.Alias : center.Alias.Trim();
                        centerRequestModel.LocationId = (checkLocationExistCenter.Id == 0) ? centerRequesting.LocationId : checkLocationExistCenter.Id;
                        centerRequestModel.Phone = string.IsNullOrWhiteSpace(center.Phone) ? centerRequesting.Phone : center.Phone.Trim();
                        centerRequestModel.Description = string.IsNullOrWhiteSpace(center.Description) ? centerRequesting.Description : center.Description.Trim();
                        centerRequestModel.MonthOff = string.IsNullOrWhiteSpace(center.MonthOff) ? centerRequesting.MonthOff : center.MonthOff.Trim();
                        centerRequestModel.IsAvailable = centerRequesting.IsAvailable;
                        centerRequestModel.Status = centerRequesting.Status;
                        centerRequestModel.Image = string.IsNullOrWhiteSpace(center.SavedFileName) ? centerRequesting.Image : center.SavedFileName.Trim();
                        centerRequestModel.HotFlag = centerRequesting.HotFlag;
                        centerRequestModel.Rating = centerRequesting.Rating;
                        centerRequestModel.NumOfRating = centerRequesting.NumOfRating;
                        centerRequestModel.CreatedDate = centerRequesting.CreatedDate;
                        centerRequestModel.CreatedBy = centerRequesting.CreatedBy;
                        centerRequestModel.UpdatedDate = DateTime.Now;
                        centerRequestModel.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    }
                    await _centerRequestService.Add(centerRequestModel);
                    centerRequesting.Status = "UpdatePending";
                    // Update
                    await _centerService.Update(centerRequesting);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = centerRequestModel
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
    }
}
