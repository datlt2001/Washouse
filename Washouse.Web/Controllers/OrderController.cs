﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Protocol;
using Washouse.Common.Helpers;
using Washouse.Common.Mails;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using Washouse.Model.ViewModel;
using Washouse.Service.Interface;
using Washouse.Web.Hubs;
using Washouse.Web.Models;

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
        private readonly IPaymentService _paymentService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletService _walletService;
        private readonly IFeedbackService _feedbackService;
        private readonly IHubContext<MessageHub> _messageHub;

        public OrderController(IOrderService orderService, ICustomerService customerService,
            IWardService wardService, ILocationService locationService, IServiceService serviceService,
            ICenterService centerService, IPromotionService promotionService, IOptions<VNPaySettings> vnpaySettings,
            INotificationService notificationService, INotificationAccountService notificationAccountService,
            IStaffService staffService, ISendMailService sendMailService, ICloudStorageService cloudStorageService,
            IPaymentService paymentService, IWalletTransactionService walletTransactionService,
            IWalletService walletService, IHubContext<MessageHub> messageHub, IFeedbackService feedbackService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _wardService = wardService;
            _locationService = locationService;
            _serviceService = serviceService;
            _centerService = centerService;
            _promotionService = promotionService;
            _vnPaySettings = vnpaySettings.Value;
            _notificationService = notificationService;
            _notificationAccountService = notificationAccountService;
            _staffService = staffService;
            _sendMailService = sendMailService;
            _cloudStorageService = cloudStorageService;
            _paymentService = paymentService;
            _walletTransactionService = walletTransactionService;
            _walletService = walletService;
            _messageHub = messageHub;
            _feedbackService = feedbackService;
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
                    if (!(User.FindFirst(ClaimTypes.Role)?.Value == null || User.FindFirst(ClaimTypes.Role)?.Value.Trim().ToLower() == "customer"))
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "User is logging in as provider",
                            Data = null
                        });
                    }
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

                    var promotion = new Promotion();
                    if (createOrderRequestModel.PromoCode != null)
                    {
                        var promo = await _promotionService.CheckValidPromoCode(createOrderRequestModel.CenterId,
                            createOrderRequestModel.PromoCode);

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
                    }

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
                    var location = new Location();
                    location.AddressString = AddressString;
                    location.WardId = createOrderRequestModel.Order.CustomerWardId;
                    location.Latitude = Latitude;
                    location.Longitude = Longitude;
                    var locationResult = await _locationService.Add(location);

                    var customerByPhone =
                        await _customerService.GetByPhone(createOrderRequestModel.Order.CustomerMobile);

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
                    else if (User.FindFirst(ClaimTypes.Role)?.Value == null && customerByPhone != null)
                    {
                        customer = customerByPhone;
                    }
                    else if (User.FindFirst(ClaimTypes.Role)?.Value.Trim().ToLower() == "customer" &&
                             User.FindFirst("Phone")?.Value.Trim() ==
                             createOrderRequestModel.Order.CustomerMobile.Trim())
                    {
                        var cus = await _customerService.GetCustomerByAccIDLightWeight(int.Parse(User.FindFirst("Id")?.Value));
                        customer.Id = cus.Id;
                    }
                    else if (User.FindFirst(ClaimTypes.Role)?.Value.Trim().ToLower() == "customer" &&
                             customerByPhone != null)
                    {
                        var cus = await _customerService.GetCustomerByAccIDLightWeight(int.Parse(User.FindFirst("Id")?.Value));
                        customer.Id = cus.Id;
                    }
                    else if (User.FindFirst(ClaimTypes.Role)?.Value.Trim().ToLower() == "customer" &&
                             User.FindFirst("Phone")?.Value.Trim() !=
                             createOrderRequestModel.Order.CustomerMobile.Trim())
                    {
                        //add Customer
                        Customer cus = new Customer();
                        cus.Fullname = createOrderRequestModel.Order.CustomerName;
                        cus.Phone = createOrderRequestModel.Order.CustomerMobile;
                        cus.Address = locationResult.Id;
                        cus.Email = createOrderRequestModel.Order.CustomerEmail;
                        cus.Status = true;
                        cus.AccountId = null;
                        cus.CreatedDate = DateTime.Now;
                        cus.CreatedBy = "AutoInsert";
                        await _customerService.Add(cus);
                        customer.Id = int.Parse(User.FindFirst("Id")?.Value);
                    }

                    var orders = await _orderService.GetAllOfDay(DateTime.Now.ToString("yyyyMMdd"));

                    int lastId = 0;
                    if (orders != null)
                    {
                        
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
                    //if (center.OperatingHours)
                    //order.PreferredDeliverTime = string.IsNullOrEmpty(createOrderRequestModel.Order.PreferredDeliverTime) ? null : DateTime.ParseExact(createOrderRequestModel.Order.PreferredDeliverTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    order.Status = "Pending";
                    order.CreatedBy = customer.Email != null
                        ? customer.Email
                        : createOrderRequestModel.Order.CustomerEmail;
                    order.CreatedDate = DateTime.Now;

                    //create List OrderDetails
                    var orderDetails = new List<OrderDetail>();
                    List<OrderDetailRequestModel> orderDetailRequestModels =
                        JsonConvert.DeserializeObject<List<OrderDetailRequestModel>>(createOrderRequestModel
                            .OrderDetails.ToJson());
                    decimal totalPayment = 0;
                    foreach (var item in orderDetailRequestModels)
                    {
                        var serviceItem = await _serviceService.GetByIdToCreateOrder(item.ServiceId);
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
                            CustomerNote = User.FindFirst(ClaimTypes.Role)?.Value == "Staff" ? null : item.CustomerNote,
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
                        var deliveryLocation = new Location();
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
                    payment.PromoCode = createOrderRequestModel.PromoCode != null ? promotion.Id : null;
                    payment.PaymentMethod = createOrderRequestModel.PaymentMethod;
                    payment.Discount = promotion != null ? promotion.Discount : 0;
                    payment.Total = payment.PromoCode != null
                        ? (totalPayment * (1 - promotion.Discount) + paymentDelivery)
                        : (totalPayment + paymentDelivery);
                    payment.CreatedDate = DateTime.Now;
                    payment.CreatedBy = "AutoInsert";

                    //List OrderTracking
                    var orderTrackings = new List<OrderTracking>();
                    orderTrackings.Add(new OrderTracking
                    {
                        OrderId = order.Id,
                        Status = "Pending",
                        CreatedDate = DateTime.Now,
                        CreatedBy = "AutoInsert",
                    });
                    //
                    var orderAdded =
                        await _orderService.Create(order, orderDetails, deliveries, payment, orderTrackings);

                    //Update Promotion UseTimes
                    if (promotion != null)
                    {
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
                    notification.Title = "Thông báo về đơn hàng:  " + orderAdded.Id;
                    notification.Content = "Đơn hàng " + orderAdded.Id + " đã được tạo và đang chờ trung tâm xác nhận.";
                    await _notificationService.Add(notification);
                    //await _messageHub.Clients.All.NotifyToUser("NotificationAdded");
                    await _messageHub.Clients.All.SendAsync("CreateOrder", notification);
                    if (id != null)
                    {
                        //var cusinfo = _customerService.GetById(orderAdded.CustomerId);
                        //var accId = cusinfo.Result.AccountId ?? 0;
                        notificationAccount.AccountId = int.Parse(id);
                        notificationAccount.NotificationId = notification.Id;
                        await _notificationAccountService.Add(notificationAccount);
                        //await _messageHub.Clients.User(id).ReceiveNotification(notification.Content);
                        //await _messageHub.Clients.User(id).SendNotification(notificationAccount.AccountId, "NotificationAdded");
                        //await _messageHub(notificationAccount.AccountId, "NotificationAdded");
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
                            //await _messageHub.Clients.All.SendAsync("CreateOrder", notification);
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
                    content = content.Replace("{phone}", orderAdded.CustomerMobile);
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
                    }
                    else if (requestModel.DeliveryType == 2)
                    {
                        loop = 1;
                        requestWardId = requestModel.DeliverWardId;
                        requestAddress = requestModel.DeliverAddress;
                    }
                    else if (requestModel.DeliveryType == 3)
                    {
                        loop = 2;
                        requestWardId = requestModel.DropoffWardId;
                        requestAddress = requestModel.DropoffAddress;
                    }

                    for (int i = 0; i < loop; i++)
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
                        string fullAddress = AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName +
                                             ", TP. Hồ Chí Minh";
                        string wardAddress = ward.WardName + ", " + ward.District.DistrictName +
                                             ", TP. Hồ Chí Minh";
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
                        var center = await _centerService.GetByIdToCalculateDeliveryPrice(centerId);
                        if (center.Location.Latitude == null || center.Location.Longitude == null || Latitude == null ||
                            Longitude == null)
                        {
                            distance = 0;
                        }
                        else
                        {
                            distance = (decimal)Utilities.CalculateDistance(Math.Round((decimal)Latitude, 6),
                                Math.Round((decimal)Longitude, 6),
                                Math.Round((decimal)center.Location.Latitude, 6),
                                Math.Round((decimal)center.Location.Longitude, 6));
                        }

                        var deliveryPriceCharts = center.DeliveryPriceCharts.Where(c => c.Status == true)
                            .OrderByDescending(a => a.MaxDistance).ThenByDescending(a => a.MaxWeight).ToList();
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
        /// Gets the list of all Orders.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     OrderType = "orderbyme"/"orderbyanother"/null or any letter.
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
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetOrders([FromQuery] FilterMyOrdersRequestModel filterOrdersRequestModel)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = User.FindFirst("Id")?.Value;
                var userPhone = User.FindFirst("Phone")?.Value;
                var customerCreateOrder = await _customerService.GetCustomerByAccID(int.Parse(userId));
                var customerByPhone = await _customerService.GetByPhone(userPhone);
                var orders = await _orderService.GetOrdersOfCustomer(customerCreateOrder.Id, userPhone);
                if (orders.Count() == 0)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
                        Data = ""
                    });
                }

                if (filterOrdersRequestModel.OrderType != null &&
                    filterOrdersRequestModel.OrderType.Trim().ToLower().Equals("orderbyme"))
                {
                    orders = orders.Where(order => order.CustomerId == customerCreateOrder.Id);
                }
                else if (filterOrdersRequestModel.OrderType != null &&
                         filterOrdersRequestModel.OrderType.Trim().ToLower().Equals("orderbyanother"))
                {
                    orders = orders.Where(order =>
                        (order.CustomerMobile.Trim().Equals(userPhone) && order.CustomerId != customerCreateOrder.Id));
                }
                else
                {
                    orders = orders.Where(order =>
                        (order.CustomerId == customerCreateOrder.Id || order.CustomerMobile.Trim().Equals(userPhone)));
                }

                if (filterOrdersRequestModel.SearchString != null)
                {
                    orders = orders.Where(order => (order.OrderDetails.Any(orderDetail =>
                                                        (orderDetail.Service.ServiceName.ToLower()
                                                             .Contains(filterOrdersRequestModel.SearchString.ToLower())
                                                         || (orderDetail.Service.Alias != null && orderDetail.Service
                                                             .Alias.ToLower()
                                                             .Contains(filterOrdersRequestModel.SearchString
                                                                 .ToLower()))))
                                                    || order.Id.ToLower()
                                                        .Contains(filterOrdersRequestModel.SearchString.ToLower())
                                                    || order.OrderDetails.FirstOrDefault().Service.Center.CenterName
                                                        .ToLower().Contains(filterOrdersRequestModel.SearchString
                                                            .ToLower())))
                        .ToList();
                }

                if (filterOrdersRequestModel.Status != null)
                {
                    orders = orders.Where(order =>
                            (order.Status.ToLower().Trim().Equals(filterOrdersRequestModel.Status.ToLower().Trim())))
                        .ToList();
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

                var response = new List<OrderCenterModel>();
                orders = orders.OrderByDescending(x => x.CreatedDate).ToList();
                foreach (var order in orders)
                {
                    decimal TotalOrderValue = 0;
                    var orderedServices = new List<OrderedServiceModel>();
                    bool checkFirst = true;
                    string CenterName = null;
                    int? CenterId = null;
                    foreach (var item in order.OrderDetails)
                    {
                        if (checkFirst)
                        {
                            //var center = await _centerService.GetById(item.Service.CenterId);
                            var center = item.Service.Center;
                            CenterId = center.Id;
                            CenterName = center.CenterName;
                            checkFirst = false;
                        }

                        TotalOrderValue += item.Price;
                        orderedServices.Add(new OrderedServiceModel
                        {
                            ServiceId = item.ServiceId,
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
                        Discount = order.Payments.Count > 0 ? order.Payments.First().Discount : 0,
                        TotalOrderPayment = order.Payments.Count > 0 ? order.Payments.First().Total : 0,
                        Status = order.Status,
                        CenterId = CenterId,
                        CenterName = CenterName,
                        IsFeedback = order.IsFeedback,
                        IsPayment = (order.Payments.Count > 0 &&
                                     order.Payments.First().Status.Trim().ToLower().Equals("paid") ||
                                     order.Payments.First().Status.Trim().ToLower().Equals("received"))
                            ? true
                            : false,
                        OrderedServices = orderedServices
                    });
                }

                int totalItems = response.Count();
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
                    List<CartItemModel> cartItems =
                        JsonConvert.DeserializeObject<List<CartItemModel>>(cartItem.ToJson());
                    foreach (var item in cartItems)
                    {
                        var service = await _serviceService.GetById(item.Id);
                        if (service.CenterId != item.CenterId)
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
                    List<CartItemModel> cartItems =
                        JsonConvert.DeserializeObject<List<CartItemModel>>(cartItem.ToJson());
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

        // GET: api/orders/{id}
        [HttpGet("search")]
        [Produces("application/json")]
        public async Task<IActionResult> GetOrderDetailOAnOrder(string OrderId, string Phone)
        {
            try
            {
                if (string.IsNullOrEmpty(OrderId) || (OrderId.Length != 16))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "OrderId format wrong",
                        Data = null
                    });
                }

                if (string.IsNullOrEmpty(Phone) || !DataValidation.CheckPhoneNumber(Phone))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Phone number format wrong",
                        Data = null
                    });
                }

                var order = await _orderService.GetOrderById(OrderId);
                if (order == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found order",
                        Data = null
                    });
                }

                if (order != null && ((order.CustomerMobile != Phone) && order.Customer.Phone != Phone))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "OrderId and Phone not matched",
                        Data = null
                    });
                }

                var response = new OrderInfomationModel();
                response.Id = OrderId;
                response.CustomerName = order.CustomerName;
                response.LocationId = order.LocationId;
                response.CustomerAddress = order.Location.AddressString + ", " + order.Location.Ward.WardName + ", " +
                                           order.Location.Ward.District.DistrictName + ", TP. Hồ Chí Minh";
                response.CustomerEmail = order.CustomerEmail;
                response.CustomerMobile = order.CustomerMobile;
                response.CustomerMessage = order.CustomerMessage;
                response.CustomerOrdered = order.CustomerId;
                response.DeliveryType = order.DeliveryType;
                response.DeliveryPrice = order.DeliveryPrice;
                response.CancelReasonByStaff = order.CancelReasonByStaff;
                response.CancelReasonByCustomer = order.CancelReasonByCustomer;
                response.PreferredDropoffTime = order.PreferredDropoffTime?.ToString("dd-MM-yyyy HH:mm:ss");
                response.PreferredDeliverTime = order.PreferredDeliverTime?.ToString("dd-MM-yyyy HH:mm:ss");
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
                var checkFirst = true;
                foreach (var item in order.OrderDetails)
                {
                    if (checkFirst)
                    {
                        var center = await _centerService.GetByIdIncludeAddress(item.Service.CenterId);
                        var Center = new CenterOfOrderModel()
                        {
                            CenterId = center.Id,
                            CenterName = center.CenterName,
                            CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName +
                                            ", " + center.Location.Ward.District.DistrictName,
                            CenterPhone = center.Phone,
                        };
                        response.Center = Center;
                        checkFirst = false;
                    }

                    TotalOrderValue += item.Price;
                    var _orderDetailTrackingModel = new List<OrderDetailTrackingModel>();
                    foreach (var tracking in item.OrderDetailTrackings)
                    {
                        _orderDetailTrackingModel.Add(new OrderDetailTrackingModel
                        {
                            Status = tracking.Status,
                            CreatedDate = tracking.CreatedDate?.ToString("dd-MM-yyyy HH:mm:ss"),
                            UpdatedDate = tracking.UpdatedDate?.ToString("dd-MM-yyyy HH:mm:ss"),
                        });
                    }

                    OrderedDetails.Add(new OrderDetailInfomationModel
                    {
                        ServiceName = item.Service.ServiceName,
                        ServiceCategory = item.Service.Category.CategoryName,
                        Measurement = item.Measurement,
                        Unit = item.Service.Unit,
                        CustomerNote = item.CustomerNote,
                        StaffNote = item.StaffNote,
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
                        CreatedDate = tracking.CreatedDate?.ToString("dd-MM-yyyy HH:mm:ss"),
                        UpdatedDate = tracking.UpdatedDate?.ToString("dd-MM-yyyy HH:mm:ss"),
                    });
                }

                response.OrderTrackings = OrderTrackings;
                foreach (var delivery in order.Deliveries)
                {
                    var location = await _locationService.GetById(delivery.LocationId);
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
                        DeliveryDate = delivery.DeliveryDate?.ToString("dd-MM-yyyy HH:mm:ss")
                    });
                }

                response.OrderDeliveries = OrderDeliveries;
                var feedback = await _feedbackService.GetByOrderId(order.Id);
                if (feedback is not null)
                {
                    response.Feedback = new FeedbackCenterModel
                    {
                        Content = feedback.Content,
                        CreatedDate = feedback.CreatedDate,
                        CreatedBy = feedback.CreatedBy,
                        Rating = feedback.Rating,
                        ReplyBy = feedback.ReplyBy,
                        ReplyDate = feedback.ReplyDate,
                        ReplyMessage = feedback.ReplyMessage
                    };
                }
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = response
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

        [Authorize(Roles = "Customer")]
        // GET: api/orders/{id}/payment
        [HttpGet("{orderId}/payment")]
        public async Task<IActionResult> PayOrder(string orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId) || (orderId.Length != 16))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "OrderId format wrong",
                        Data = null
                    });
                }

                var order = await _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found order",
                        Data = null
                    });
                }

                if (!order.Status.ToLower().Trim().Equals("ready"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Order status not ready",
                        Data = null
                    });
                }

                if (order.Payments.First().Status.ToLower().Trim().Equals("paid"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Order has been already paid",
                        Data = null
                    });
                }

                if (order.OrderDetails.FirstOrDefault().Service.Center.WalletId == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Wallet payment to this center is not available",
                        Data = null
                    });
                }

                var userId = int.Parse(User.FindFirst("Id")?.Value);
                var customer = await _customerService.GetCustomerByAccID(userId);
                if (order != null && (order.CustomerId != customer.Id))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Do not accept you pay this order",
                        Data = null
                    });
                }

                if (customer.Account.WalletId == null ||
                    !customer.Account.Wallet.Status.ToLower().Trim().Equals("active"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Your wallet is not available",
                        Data = null
                    });
                }

                if (customer.Account.Wallet.Balance < order.Payments.FirstOrDefault().Total)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Your wallet not enough balance",
                        Data = null
                    });
                }

                var walletPay = customer.Account.Wallet;
                walletPay.Balance = walletPay.Balance - order.Payments.FirstOrDefault().Total;
                await _walletService.Update(walletPay);

                var payment = order.Payments.First();
                payment.Status = "Paid";
                await _paymentService.Update(payment);

                var walletTransaction = new WalletTransaction();
                walletTransaction.PaymentId = payment.Id;
                walletTransaction.Type = "PayOrder";
                walletTransaction.Status = "Paid";
                walletTransaction.FromWalletId = (int)customer.Account.WalletId;
                walletTransaction.ToWalletId = (int)order.OrderDetails.FirstOrDefault().Service.Center.WalletId;
                walletTransaction.Amount = order.Payments.FirstOrDefault().Total;
                walletTransaction.TimeStamp = DateTime.Now;
                await _walletTransactionService.Add(walletTransaction);

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