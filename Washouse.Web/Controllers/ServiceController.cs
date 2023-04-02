﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public ServiceController(ICenterService centerService, IServiceService serviceService, ICloudStorageService cloudStorageService)
        {
            this._centerService = centerService;
            this._serviceService = serviceService;
            this._cloudStorageService = cloudStorageService;
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
            catch {
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
                        NumOfRating = item.NumOfRating
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
            } catch {   
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
                    if (string.IsNullOrEmpty(User.FindFirst("CenterManaged")?.Value))
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
                    serviceRequest.Status = "CreatePending";
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
                        List<ServicePriceViewModel> servicePrices = JsonConvert.DeserializeObject<List<ServicePriceViewModel>>(serviceRequestmodel.Prices.ToJson());
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
                    List<string> serviceGalleries = JsonConvert.DeserializeObject<List<string>>(serviceRequestmodel.ServiceGalleries.ToJson());
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
                    List<ServicePriceViewModel> servicePriceList = JsonConvert.DeserializeObject<List<ServicePriceViewModel>>(serviceRequestmodel.Prices.ToJson());
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
                            Image = result.Image != null ? await _cloudStorageService.GetSignedUrlAsync(result.Image) : null,
                            PriceType = result.PriceType,
                            Price = result.Price,
                            MinPrice = result.MinPrice,
                            Unit = result.Unit,
                            Rate = result.Rate,
                            Prices = servicePriceList,
                            TimeEstimate = result.TimeEstimate,
                            Rating = result.Rating,
                            NumOfRating = result.NumOfRating
                        }
                    });
                }
                else { return BadRequest(); }
            }
            catch {
                return BadRequest();
            }
        }
    }
}
