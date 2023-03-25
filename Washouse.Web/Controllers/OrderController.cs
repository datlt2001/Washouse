using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using Microsoft.CodeAnalysis;
using System.Linq;
using Washouse.Common.Helpers;
using NuGet.Protocol;

namespace Washouse.Web.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        #region Initialize
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IWardService _wardService;
        private readonly ILocationService _locationService;
        private readonly IServiceService _serviceService;
        private readonly ICenterService _centerService;
        public OrderController(IOrderService orderService, ICustomerService customerService, 
            IWardService wardService, ILocationService locationService, IServiceService serviceService, ICenterService centerService)
        {
            this._orderService = orderService;
            this._customerService = customerService;
            this._wardService = wardService;
            this._locationService = locationService;
            this._serviceService = serviceService;
            this._centerService = centerService;
        }
        #endregion

        //POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestModel createOrderRequestModel)
        {
            try
            {
                var order = new Order();
                var customer = new Customer();
                if (ModelState.IsValid)
                {
                    //tính tổng khối lượng
                    decimal weight = 0;
                    foreach (var item in createOrderRequestModel.OrderDetails.ToList())
                    {
                        var service = await _serviceService.GetById(item.ServiceId);
                        if (service.Unit.ToLower().Equals("kg"))
                        {
                            weight += item.Quantity;
                        }
                        else
                        {
                            weight = weight + ((decimal)(0.3) * item.Quantity);
                        }
                    }
                    //location
                    decimal? Latitude = null;
                    decimal? Longitude = null;
                    var ward = await _wardService.GetWardById(createOrderRequestModel.Order.CustomerWardId);
                    string AddressString = createOrderRequestModel.Order.CustomerAddressString;
                    string fullAddress = AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                    string wardAddress = ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
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

                    //add Order

                    if (User.FindFirst(ClaimTypes.Role).Value == null)
                    {
                        customer.Fullname = createOrderRequestModel.Order.CustomerName;
                        customer.Phone = createOrderRequestModel.Order.CustomerMobile;
                        customer.Address = locationResult.Id;
                        customer.Email = createOrderRequestModel.Order.CustomerEmail;
                        await _customerService.Add(customer);
                    } else if (User.FindFirst(ClaimTypes.Role).Value == "Customer")
                    {
                        customer.Id = int.Parse(User.FindFirst("Id")?.Value);
                    }


                    order.CustomerName = createOrderRequestModel.Order.CustomerName;
                    order.LocationId = locationResult.Id;
                    order.CustomerEmail = createOrderRequestModel.Order.CustomerEmail;
                    order.CustomerMobile = createOrderRequestModel.Order.CustomerMobile;
                    order.CustomerMessage = createOrderRequestModel.Order.CustomerMessage;
                    order.CustomerId = customer.Id;
                    if (createOrderRequestModel.Order.DeliveryChoosen == true)
                    {

                    } else
                    {
                        order.DeliveryPrice = null;
                    }
                    order.Status = "Received";
                }
                //List<OperatingHoursRequestModel> operatings = JsonConvert.DeserializeObject<List<OperatingHoursRequestModel>>(createCenterRequestModel.CenterOperatingHours.ToJson());

                return Ok(new ResponseModel
                    {
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

        [HttpGet("getDeliveryPrice")]
        //public async Task<IActionResult> GetDeliveryPrice([FromQuery]GetDeliveryPriceRequestModel requestModel)
        public async Task<IActionResult> GetDeliveryPrice([FromQuery] GetDeliveryPriceRequestModel requestModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    decimal weight = 0;
                    List<OrderDetailRequestModel> orderDetails = JsonConvert.DeserializeObject<List<OrderDetailRequestModel>>(requestModel.OrderDetails.ToJson());
                    foreach (var item in orderDetails)
                    {
                        var service = await _serviceService.GetById(item.ServiceId);
                        if (service.Unit.ToLower().Equals("kg"))
                        {
                            weight += item.Quantity;
                        }
                        else
                        {
                            weight = weight + ((decimal)(0.3) * item.Quantity);
                        }
                    }

                    decimal? Latitude = null;
                    decimal? Longitude = null;
                    var ward = await _wardService.GetWardById(requestModel.CustomerWardId);
                    string AddressString = requestModel.CustomerAddressString;
                    string fullAddress = AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                    string wardAddress = ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
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


                    decimal distance = 0;
                    int centerId = requestModel.CenterId;
                    var center = await _centerService.GetById(centerId);
                    if (center.Location.Latitude == null || center.Location.Longitude == null || Latitude == null || Longitude == null)
                    {
                        distance = 0;
                    }
                    else
                    {
                        distance = (decimal)Utilities.CalculateDistance(Math.Round((decimal)Latitude, 6), Math.Round((decimal)Longitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
                    }

                    var deliveryPriceCharts = center.DeliveryPriceCharts.OrderByDescending(a => a.MaxDistance).ThenByDescending(a => a.MaxWeight).ToList();
                    var firstItem = deliveryPriceCharts.FirstOrDefault();
                    if (weight > firstItem.MaxWeight || distance > firstItem.MaxDistance)
                    {
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Weight or distance out of range available delivery. Please call center.",
                            Data = null
                        });
                    }
                    decimal price = 0;
                    foreach (var item in center.DeliveryPriceCharts)
                    {
                        if (distance <= item.MaxDistance && weight <= item.MaxWeight)
                        {
                            price = item.Price;
                        }
                    }
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            deliveryPrice = price
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
            string url = $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={query}&format=json&limit=1";
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
    }
}
