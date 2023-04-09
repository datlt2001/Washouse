using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Common.Mails;
using Washouse.Data;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/tracking")]
    [ApiController]
    public class TrackingController : ControllerBase
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
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IOrderDetailTrackingService _orderDetailTrackingService;

        public TrackingController(IOrderService orderService, ICustomerService customerService,
            IWardService wardService, ILocationService locationService, IServiceService serviceService,
            ICenterService centerService, IPromotionService promotionService, IOptions<VNPaySettings> vnpaySettings,
            INotificationService notificationService, INotificationAccountService notificationAccountService,
            IStaffService staffService, ISendMailService sendMailService, ICloudStorageService cloudStorageService, IOrderDetailTrackingService orderDetailTrackingService)
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
            this._orderDetailTrackingService = orderDetailTrackingService;
            this._cloudStorageService = cloudStorageService;

        }
        #endregion

        [HttpPut]
        [Route("orders/{orderId}/tracking")]
        public async Task<IActionResult> UpdateOrderStatus(string orderId)
        {
            
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found order",
                    Data = null
                });
            } else
            {
                if (order.Status.ToLower().Trim().Equals("cancelled"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "order had been cancelled",
                        Data = null
                    });
                }

                string Status = Utilities.OrderNextStatus(order.Status);
                
                if (Status.ToLower().Trim().Equals("processing"))
                {
                    foreach (var item in order.OrderDetails)
                    {
                        var orderDetailTracking = new OrderDetailTracking
                        {
                            OrderDetailId = item.Id,
                            Status = "pending",
                            CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                            CreatedDate = DateTime.Now
                        };
                        await _orderDetailTrackingService.Add(orderDetailTracking);
                    }
                }
                order.Status = Status.Trim().ToLower();
                order.UpdatedDate = DateTime.Now;
                order.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                await _orderService.Update(order);
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = new
                    {
                        orderId = order.Id
                    }
                });
            }
        }

        [HttpPut]
        [Route("orders/{orderId}/cancelled")]
        public async Task<IActionResult> CancelOrder(string orderId)
        {

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
            else
            {
                if (order.Status.ToLower().Trim().Equals("cancelled"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "order had been cancelled",
                        Data = null
                    });
                }
                if (!order.Status.ToLower().Trim().Equals("pending"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "order have status that can not cancel",
                        Data = null
                    });
                }
                order.Status = "Cancelled";
                order.UpdatedDate = DateTime.Now;
                order.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                await _orderService.Update(order);
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = new
                    {
                        orderId = order.Id
                    }
                });
            }
        }

        [HttpPut]
        [Route("orders/{orderId}/order-details/{orderDetailId}/tracking")]
        public async Task<IActionResult> UpdateOrderDetailStatus(string orderId, int orderDetailId)
        {
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
            else if (order != null && order.OrderDetails == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found any order detail",
                    Data = null
                });
            }
            else
            {



                var orderDetail = order.OrderDetails.FirstOrDefault(order => order.Id == orderDetailId);
                string Status = Utilities.OrderDetailNextStatus(orderDetail.Status);

                if (DataValidation.CheckValidUpdateOrderDetailStatus(Status) == false)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "status is not valid",
                        Data = null
                    });
                }

                if (orderDetail.OrderDetailTrackings.Last().Status.ToLower().Trim().Equals(Status.ToLower().Trim()))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "status no change",
                        Data = null
                    });
                }
                if (Status.ToLower().Trim().Equals("completed"))
                {
                    int count = order.OrderDetails.Where(od => od.Status.ToLower().Trim().Equals("completed") == false).Count();
                    if (count == 1) {
                        order.Status = "ready";
                        order.UpdatedDate = DateTime.Now;
                        order.UpdatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                        await _orderService.Update(order);
                    }
                }

                var orderDetailTracking = new OrderDetailTracking
                {
                    OrderDetailId = orderDetailId,
                    Status = Status,
                    CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                    CreatedDate = DateTime.Now
                };
                await _orderDetailTrackingService.Add(orderDetailTracking);
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = new
                    {
                        orderDetailId = orderDetail.Id
                    }
                });
            }
        }
    }
}
