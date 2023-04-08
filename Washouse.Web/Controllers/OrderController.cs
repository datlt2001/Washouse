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
using Washouse.Model.ResponseModels;
using System.Globalization;
using Washouse.Common.Utils;
using Microsoft.Extensions.Options;
using Washouse.Model.ViewModel;
using static Google.Apis.Requests.BatchRequest;
using Org.BouncyCastle.Bcpg;
using Washouse.Common.Mails;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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
        private readonly VNPaySettings _vnPaySettings;
        private readonly INotificationService _notificationService;
        private readonly INotificationAccountService _notificationAccountService;
        private readonly IStaffService _staffService;
        private ISendMailService _sendMailService;
        private readonly ICloudStorageService _cloudStorageService;
        
        public OrderController(IOrderService orderService, ICustomerService customerService, 
            IWardService wardService, ILocationService locationService, IServiceService serviceService,
            ICenterService centerService, IPromotionService promotionService, IOptions<VNPaySettings> vnpaySettings,
            INotificationService notificationService, INotificationAccountService notificationAccountService, 
            IStaffService staffService, ISendMailService sendMailService, ICloudStorageService cloudStorageService)
        {
            this._orderService = orderService;
            this._customerService = customerService;
            this._wardService = wardService;
            this._locationService = locationService;
            this._serviceService = serviceService;
            this._centerService = centerService;
            this._promotionService = promotionService;
            this._vnPaySettings = vnpaySettings.Value;
            this._notificationService = notificationService;
            this._notificationAccountService = notificationAccountService;
            this._staffService = staffService;
            this._sendMailService = sendMailService;
            this._cloudStorageService = cloudStorageService;

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
                    var center = await _centerService.GetById(createOrderRequestModel.CenterId);
                    DateTime UserPreferredDropoffTime;
                    DateTime UserPreferredDeliverTime;
                    if (!string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDropoffTime))
                    {
                        try
                        {
                            UserPreferredDropoffTime = DateTime.ParseExact(createOrderRequestModel.Order.PreferredDropoffTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
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
                    } else
                    {
                        UserPreferredDropoffTime = DateTime.Now;
                    }
                    if (!string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDeliverTime))
                    {
                        try
                        {
                            UserPreferredDeliverTime = DateTime.ParseExact(createOrderRequestModel.Order.PreferredDeliverTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "PreferredDeliverTime: " + ex.Message,
                                Data = null
                            });
                            //Console.WriteLine("Failed to parse date: " + ex.Message);
                            // handle the parse failure
                        }
                    }
                    if (center == null)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "CenterId is not valid",
                            Data = null
                        });
                    } else
                    {
                        var openTimePreferredDropoffDay = center.OperatingHours.FirstOrDefault(day => day.DaysOfWeekId == (int)UserPreferredDropoffTime.DayOfWeek);
                        //kiểm tra ngày giờ lấy hàng không là giờ hoạt động
                        if (openTimePreferredDropoffDay == null || openTimePreferredDropoffDay.OpenTime > UserPreferredDropoffTime.TimeOfDay || openTimePreferredDropoffDay.CloseTime < UserPreferredDropoffTime.TimeOfDay)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Center is closed at time that you choosen.",
                                Data = null
                            });
                        }

                    }
                    var promotion = new Promotion();
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
                    } else
                    {
                        promotion = null;
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
                    
                    order.PreferredDropoffTime = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDropoffTime) ? null : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDropoffTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    order.PreferredDeliverTime = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDeliverTime) ? null : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDeliverTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    order.Status = "Pending";
                    order.CreatedBy = customer.Email != null ? customer.Email : createOrderRequestModel.Order.CustomerEmail;
                    order.CreatedDate = DateTime.Now;

                    //create List OrderDetails
                    var orderDetails = new List<OrderDetail>();
                    List<OrderDetailRequestModel> orderDetailRequestModels = JsonConvert.DeserializeObject<List<OrderDetailRequestModel>>(createOrderRequestModel.OrderDetails.ToJson());
                    decimal totalPayment = 0;
                    foreach (var item in orderDetailRequestModels)
                    {
                        var serviceItem = await _serviceService.GetById(item.ServiceId); 
                        if (serviceItem == null && (!serviceItem.Status.ToLower().Equals("UpdatePending") || !!serviceItem.Status.ToLower().Equals("Active")))
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Service is not valid",
                                Data = null
                            });
                        } else
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
                            } else
                            {
                                totalCurrentPrice = currentPrice * item.Measurement;
                            }
                        } else
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
                            CustomerNote = User.FindFirst(ClaimTypes.Role)?.Value == "Staff" ? null : item.CustomerNote,
                            StaffNote = User.FindFirst(ClaimTypes.Role)?.Value == "Staff" ? item.StaffNote : null,
                        });

                        totalPayment = totalPayment + item.Price;
                    }

                    //create List Deliveries
                    
                    var deliveries = new List<Delivery>();
                    List<DeliveryRequestModel> deliveryRequestModels = JsonConvert.DeserializeObject<List<DeliveryRequestModel>>(createOrderRequestModel.Deliveries.ToJson());

                    foreach (var item in deliveryRequestModels)
                    {
                        //location
                        decimal? deliveryLatitude = null;
                        decimal? deliveryLongitude = null;
                        var deliveryWard = await _wardService.GetWardById(item.WardId);
                        string deliveryAddressString = item.AddressString;
                        string fullDeliveryAddress = deliveryAddressString + ", " + deliveryWard.WardName + ", " + deliveryWard.District.DistrictName + ", Thành phố Hồ Chí Minh";
                        string wardDeliveryAddress = deliveryWard.WardName + ", " + deliveryWard.District.DistrictName + ", Thành phố Hồ Chí Minh";
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
                        var estimatedTime = await Utilities.CalculateDeliveryEstimatedTime(Math.Round((decimal)deliveryLatitude, 6), Math.Round((decimal)deliveryLongitude, 6),
                                                                Math.Round((decimal)center.Location.Latitude, 6), Math.Round((decimal)center.Location.Longitude, 6));
                        DateTime? deliveryDate = null;
                        // Dropoff = false, Deliver = true
                        if (!item.DeliveryType)
                        {
                            deliveryDate = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDropoffTime) ? null : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDropoffTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                            if (deliveryDate == null)
                            {
                                deliveryDate = DateTime.Now;
                            }
                        } else
                        {
                            deliveryDate = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDeliverTime) ? null : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDeliverTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
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
                            Status = "Waiting",
                            CreatedBy = customer.Email != null ? customer.Email : createOrderRequestModel.Order.CustomerEmail,
                            CreatedDate = DateTime.Now
                        });
                    }
                    
                    //create Payment
                    var payment = new Payment();
                    payment.OrderId = order.Id;
                    payment.PlatformFee = Utilities.platformFee;
                    payment.Date = null;
                    payment.Status = "Pending";
                    payment.PromoCode = createOrderRequestModel.PromoCode != null ? promotion.Id : null;
                    payment.PaymentMethod = createOrderRequestModel.PaymentMethod;
                    payment.Total = payment.PromoCode != null ? totalPayment * (1-promotion.Discount) : totalPayment;
                    payment.CreatedDate = DateTime.Now;
                    payment.CreatedBy = "AutoInsert";

                    //OrderTracking
                    var orderTracking = new OrderTracking();
                    orderTracking.OrderId = order.Id;
                    orderTracking.Status = "Pending";
                    orderTracking.CreatedDate = DateTime.Now;
                    orderTracking.CreatedBy = "AutoInsert";



                    //
                    var orderAdded = await _orderService.Create(order, orderDetails, deliveries, payment, orderTracking);

                    //Update Promotion UseTimes
                    if (promotion != null) {
                        promotion.UseTimes = promotion.UseTimes - 1;
                        await _promotionService.Update(promotion);
                    }

                    //

                    //notification
                    string id = User.FindFirst("Id")?.Value;
                    Notification notification = new Notification();
                    NotificationAccount notificationAccount = new NotificationAccount();
                    notification.OrderId = orderAdded.Id;
                    notification.CreatedDate = DateTime.Now;
                    notification.Content = "Order " + orderAdded.Id + " đã được tạo";
                    await _notificationService.Add(notification);

                    if (id != null)
                    {                                                                     
                        //var cusinfo = _customerService.GetById(orderAdded.CustomerId);
                        //var accId = cusinfo.Result.AccountId ?? 0;
                        notificationAccount.AccountId = int.Parse(id);
                        notificationAccount.NotificationId = notification.Id;
                        await _notificationAccountService.Add(notificationAccount);
                    }
                    var staff =  _staffService.GetAllByCenterId(createOrderRequestModel.CenterId);
                    if(staff != null)
                    {
                        foreach(var staffItem in staff)
                        {
                            notificationAccount.AccountId = staffItem.AccountId;
                            notificationAccount.NotificationId = notification.Id;
                            await _notificationAccountService.Add(notificationAccount);
                        }
                    }

                    //string path = "./Templates_email/CreateOrder.txt";
                    //string physicalPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "site", "wwwroot", path.TrimStart('/').Replace('/', '\\'));
                    //string content = System.IO.File.ReadAllText(physicalPath);
                    //content = content.Replace("{recipient}", customer.Fullname);

                    //content = content.Replace("{orderId}", orderAdded.Id);
                    //await _sendMailService.SendEmailAsync(customer.Email, "Tạo đơn hàng", content);
                    






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

        [HttpGet("delivery-price")]
        public async Task<IActionResult> GetDeliveryPrice([FromQuery] GetDeliveryPriceRequestModel requestModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (requestModel.DeliveryType == 0)
                    {
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Delivery type not choosen",
                            Data = new
                            {
                                deliveryPrice = 0
                            }
                        });
                    }
                    decimal weight = requestModel.TotalWeight;
                    int requestWardId = 0;
                    string requestAddress = null;
                    int loop = 0;
                    decimal priceTotal = -1;
                    decimal price1 = 0;
                    decimal price2 = 0;
                    decimal price3 = 0;
                    if (requestModel.DeliveryType == 1)
                    {
                        loop = 1;
                        requestWardId = requestModel.DropoffWardId;
                        requestAddress = requestModel.DropoffAddress;
                    } else if (requestModel.DeliveryType == 2)
                    {
                        loop = 1;
                        requestWardId = requestModel.DeliverWardId;
                        requestAddress = requestModel.DeliverAddress;
                    } else if (requestModel.DeliveryType == 3)
                    {
                        loop = 2;
                        requestWardId = requestModel.DropoffWardId;
                        requestAddress = requestModel.DropoffAddress;
                    }
                    for (int i = 0; i< loop; i++)
                    {
                        if (i == 1)
                        {
                            requestWardId = requestModel.DeliverWardId;
                            requestAddress = requestModel.DeliverAddress;
                        }
                        decimal? Latitude = null;
                        decimal? Longitude = null;
                        var ward = await _wardService.GetWardById(requestWardId);
                        string AddressString = requestAddress;
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
                                break;
                            }
                        }
                        if (requestModel.DeliveryType == 1)
                        {
                            price1 = price;
                        }
                        else if (requestModel.DeliveryType == 2)
                        {
                            price2 = price;
                        }
                        else if (requestModel.DeliveryType == 3)
                        {
                            price3 += price;
                        }
                    }
                    if (requestModel.DeliveryType == 1)
                    {
                        priceTotal = price1;
                    }
                    else if (requestModel.DeliveryType == 2)
                    {
                        priceTotal = price2;
                    }
                    else if (requestModel.DeliveryType == 3)
                    {
                        priceTotal = price3;
                    }
                    if (priceTotal < 0)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Error occur",
                            Data = null
                        });
                    }
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            deliveryPrice = priceTotal
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
        /// <response code="200">Success return list ceters</response>   
        /// <response code="404">Not found any center matched</response>   
        /// <response code="400">One or more error occurs</response>   
        // GET: api/orders
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll([FromQuery] FilterOrdersRequestModel filterOrdersRequestModel)
        {
            try
            {
                var orders = await _orderService.GetAll();
                if (orders.Count() == 0)
                {
                    return BadRequest();
                }

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role != null && role.Trim().ToLower().Equals("customer"))
                {
                    var userId = User.FindFirst("Id")?.Value;
                    var userPhone = User.FindFirst("Phone")?.Value;
                    var customer = _customerService.GetByPhone(userPhone);
                    orders = orders.Where(orders => orders.CustomerId == int.Parse(userId));
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
                    var orderedServices = new List<OrderedServiceModel>();
                    foreach (var item in order.OrderDetails)
                    {
                        TotalOrderValue += item.Price;
                        orderedServices.Add(new OrderedServiceModel
                        {
                            ServiceName = item.Service.ServiceName,
                            Measurement = item.Measurement,
                            Unit = item.Service.Unit,
                            Image = item.Service.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Service.Image) : null,
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
                        Discount = order.Payments.Count > 0 ? order.Payments.First().Discount : 0,
                        TotalOrderPayment = order.Payments.Count > 0 ? order.Payments.First().Total : 0,
                        Status = order.Status,
                        OrderedServices = orderedServices
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
                            /*TotalItems = totalItems,
                            TotalPages = totalPages,*/
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

        [HttpPost("estimated-time")]
        public async Task<IActionResult> GetEstimatedTime([FromBody] List<CartItemModel> cartItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int totalEstimatedTime = 0;
                    var services = new List<Model.Models.Service>();
                    List<CartItemModel> cartItems = JsonConvert.DeserializeObject<List<CartItemModel>>(cartItem.ToJson());
                    foreach (var item in cartItems)
                    {
                        var service = await _serviceService.GetById(item.Id);
                        if(service.CenterId != item.CenterId)
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Error by service not in this center or service is unavailable.",
                                Data = null
                            });
                        }
                        if (service.TimeEstimate != null)
                        {
                            totalEstimatedTime = totalEstimatedTime + (int)service.TimeEstimate;
                        }
                    }
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalEstimatedTime = totalEstimatedTime
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

        [HttpPost("total-price")]
        public async Task<IActionResult> GetTotalPrice([FromBody] List<CartItemModel> cartItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var services = new List<Model.Models.Service>();
                    List<CartItemModel> cartItems = JsonConvert.DeserializeObject<List<CartItemModel>>(cartItem.ToJson());
                    decimal response = 0;
                    foreach (var item in cartItems)
                    {
                        decimal currentPrice = 0;
                        decimal totalCurrentPrice = 0;
                        if (item.PriceType)
                        {
                            var priceChart = item.PriceChart.OrderBy(a => a.MaxValue).ToList();
                            bool check = false;
                            foreach (var itemSerivePrice in priceChart)
                            {
                                if (item.Quantity <= itemSerivePrice.MaxValue && !check)
                                {
                                    currentPrice = itemSerivePrice.Price;
                                }
                                if (currentPrice > 0)
                                {
                                    check = true;
                                }
                            }
                            if (currentPrice * item.Quantity < item.MinPrice)
                            {
                                totalCurrentPrice = (decimal)item.MinPrice;
                            }
                            else
                            {
                                totalCurrentPrice = currentPrice * (decimal)item.Quantity;
                            }
                        }
                        else
                        {
                            totalCurrentPrice = item.Price * (decimal)item.Quantity;
                        }
                        response = response + totalCurrentPrice;
                    }
                    return Ok(new ResponseModel                                   
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalCartPrice = response
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


    }
}
