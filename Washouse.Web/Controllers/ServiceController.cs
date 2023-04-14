using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServiceController : ControllerBase
    {
        #region Initialize

        private readonly ICenterService _centerService;
        private readonly IServiceService _serviceService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IStaffService _staffService;
        private readonly IFeedbackService _feedbackService;

        public ServiceController(ICenterService centerService, IServiceService serviceService,
            IFeedbackService feedbackService, ICloudStorageService cloudStorageService,
            IStaffService staffService)
        {
            this._centerService = centerService;
            this._serviceService = serviceService;
            this._feedbackService = feedbackService;
            this._cloudStorageService = cloudStorageService;
            _staffService = staffService;
        }

        #endregion

        //[Route("GetServicesOfACenter")]
        [HttpGet("center/{id}")]
        public async Task<IActionResult> GetServicesOfACenter(int id)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    var services = _serviceService.GetAll().Result.Where(a => a.CenterId == id);
                    if (services != null)
                    {
                        return Ok(services);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch
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
                var item = await _serviceService.GetById(id);
                if (item != null)
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
                        Prices = servicePriceViewModels.OrderByDescending(a => a.Price).ToList(),
                        TimeEstimate = item.TimeEstimate,
                        Rating = item.Rating,
                        NumOfRating = item.NumOfRating,
                        Ratings = new int[] { st1, st2, st3, st4, st5 }
                    };
                    return Ok(new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = itemResponse
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found service",
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

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateService(int id)
        {
            try
            {
                var service = await _serviceService.GetById(id);
                if (service == null)
                {
                    return NotFound();
                }

                await _serviceService.DeactivateService(id);
                return Ok();
            }
            catch
            {
                return BadRequest();
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
        [HttpPost]
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

        [Authorize(Roles = "Manager")]
        [HttpPut("{serviceId:int}")]
        public async Task<IActionResult> UpdateService([FromRoute] int serviceId,
            [FromBody] UpdateServiceRequestModel updateServiceRequestModel)
        {
            try
            {
                var staffInfo = await _staffService.GetByAccountId(int.Parse(User.FindFirst("Id")?.Value));
                var center = await _centerService.GetById((int)staffInfo.CenterId);

                var service = await _serviceService.GetById(serviceId);
                if (service == null || !Equals(center.Id, service.CenterId))
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Service not found",
                        Data = null
                    });
                }

                if (!string.IsNullOrWhiteSpace(updateServiceRequestModel.Description))
                {
                    service.Description = Strings.Trim(updateServiceRequestModel.Description);
                }

                if (!string.IsNullOrWhiteSpace(updateServiceRequestModel.Image))
                {
                    service.Image = Strings.Trim(updateServiceRequestModel.Image);
                }

                if (updateServiceRequestModel.TimeEstimate != null)
                {
                    service.TimeEstimate = updateServiceRequestModel.TimeEstimate;
                }

                if (!service.PriceType && updateServiceRequestModel.Price != null)
                {
                    service.Price = updateServiceRequestModel.Price;
                }

                if (updateServiceRequestModel.MinPrice != null)
                {
                    service.MinPrice = updateServiceRequestModel.MinPrice;
                }

                if (service.PriceType && updateServiceRequestModel.ServicePrices != null
                                      && !updateServiceRequestModel.ServicePrices.IsNullOrEmpty())
                {
                    service.ServicePrices = updateServiceRequestModel.ServicePrices
                        .ToList().ConvertAll(sp =>
                            new ServicePrice
                            {
                                Price = sp.Price,
                                MaxValue = sp.MaxValue
                            });
                }
                _serviceService.Update(service);

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("category/{id}")]
        public IActionResult GetServiceByCate(int id)
        {
            try
            {
                var service = _serviceService.GetServicesByCategory(id);
                if (service == null)
                {
                    return NotFound();
                }

                //await _serviceService.DeactivateService(id);
                return Ok(service);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}