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
        private readonly IPromotionService _promotionService;
        public OrderController(IOrderService orderService, ICustomerService customerService, 
            IWardService wardService, ILocationService locationService, IServiceService serviceService,
            ICenterService centerService, IPromotionService promotionService)
        {
            this._orderService = orderService;
            this._customerService = customerService;
            this._wardService = wardService;
            this._locationService = locationService;
            this._serviceService = serviceService;
            this._centerService = centerService;
            this._promotionService = promotionService;
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

                    var customerByPhone = await _customerService.GetByPhone(createOrderRequestModel.Order.CustomerMobile);

                    if (User.FindFirst(ClaimTypes.Role)?.Value == null && customerByPhone == null)
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
                    else if(User.FindFirst(ClaimTypes.Role)?.Value == null && customerByPhone != null)
                    {
                        customer = customerByPhone;
                    }
                    else if (User.FindFirst(ClaimTypes.Role)?.Value == "Customer")
                    {
                        customer.Id = int.Parse(User.FindFirst("Id")?.Value);
                    }
                    
                    var orders = await _orderService.GetAllOfDay(DateTime.Now.ToString("yyyyMMdd"));
                    
                    int lastId = 0;
                    if (orders.ToList().Count > 0)
                    {
                        var lastOrder = orders.LastOrDefault();
                        lastId = int.Parse(lastOrder.Id.Substring(10));
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
                    } else
                    {
                        order.DeliveryPrice = null;
                    }
                    order.Status = "Received";
                    order.CreatedBy = customer.Email != null ? customer.Email : createOrderRequestModel.Order.CustomerEmail;
                    order.CreatedDate = DateTime.Now;

                    //create List OrderDetails
                    var orderDetails = new List<OrderDetail>();
                    List<OrderDetailRequestModel> orderDetailRequestModels = JsonConvert.DeserializeObject<List<OrderDetailRequestModel>>(createOrderRequestModel.OrderDetails.ToJson());

                    foreach (var item in orderDetailRequestModels)
                    {
                        orderDetails.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            ServiceId = item.ServiceId,
                            Measurement = item.Measurement,
                            Price = item.Price,
                            CustomerNote = User.FindFirst(ClaimTypes.Role)?.Value == "Staff" ? null : item.CustomerNote,
                            StaffNote = User.FindFirst(ClaimTypes.Role)?.Value == "Staff" ? item.StaffNote : null,
                        });
                    }

                    var orderAdded = await _orderService.Create(order, orderDetails);


                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "sucess",
                        Data = orderAdded
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

        [HttpGet("delivery-price")]
        public async Task<IActionResult> GetDeliveryPrice([FromQuery] GetDeliveryPriceRequestModel requestModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    decimal weight = requestModel.TotalWeight;

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

                    var deliveryPriceCharts = center.DeliveryPriceCharts.Where(c => c.Status == true).OrderByDescending(a => a.MaxDistance).ThenByDescending(a => a.MaxWeight).ToList();
                    if (deliveryPriceCharts.Count <= 0) 
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Center has not delivery available.",
                            Data = null
                        });
                    }
                    var firstItem = deliveryPriceCharts.FirstOrDefault();
                    if (weight > firstItem.MaxWeight || distance > firstItem.MaxDistance)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Weight or distance out of range available delivery. Please call center.",
                            Data = null
                        });
                    }
                    decimal price = -1;
                    foreach (var item in center.DeliveryPriceCharts)
                    {
                        if (distance <= item.MaxDistance && weight <= item.MaxWeight)
                        {
                            price = item.Price;
                        }
                    }
                    if (requestModel.DeliveryType == 0)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Delivery type not choosen",
                            Data = null
                        });
                    }
                    else if (requestModel.DeliveryType == 3)
                    {
                        price = price * 2;
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
