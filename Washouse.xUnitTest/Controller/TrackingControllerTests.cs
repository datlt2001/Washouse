using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Common.Mails;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Hubs;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class TrackingControllerTests
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IWardService _wardService;
        private readonly ILocationService _locationService;
        private readonly IServiceService _serviceService;
        private readonly ICenterService _centerService;
        private readonly IPromotionService _promotionService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly INotificationAccountService _notificationAccountService;
        private readonly IStaffService _staffService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IOrderDetailTrackingService _orderDetailTrackingService;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IHubContext<MessageHub> messageHub;
        private  TrackingController _controller;
        IOptions<VNPaySettings> vnpaySettings;
        ISendMailService sendMailService;

        public TrackingControllerTests()
        {
            _orderService = A.Fake<IOrderService>();
            _customerService = A.Fake<ICustomerService>();
            _walletService= A.Fake<IWalletService>();
            _locationService   = A.Fake<ILocationService>();
            _serviceService= A.Fake<IServiceService>();
            _centerService= A.Fake<ICenterService>();
            _promotionService = A.Fake<IPromotionService>();
            _paymentService = A.Fake<IPaymentService>();
            _notificationService= A.Fake<INotificationService>();
            _notificationAccountService= A.Fake<INotificationAccountService>();
            _staffService = A.Fake<IStaffService>();
            _cloudStorageService= A.Fake<ICloudStorageService>();
            _orderDetailTrackingService =A.Fake<IOrderDetailTrackingService>();
            _walletService = A.Fake<IWalletService>();
            _walletTransactionService= A.Fake<IWalletTransactionService>();
            messageHub = A.Fake<IHubContext<MessageHub>>();
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
               {
                    new Claim(ClaimTypes.Email, "test@example.com"),
                    new Claim(ClaimTypes.Role, "Manager"),
                    new Claim("Id", "1")
               }, "mock"));
            _controller = new TrackingController(_orderService, _customerService, _wardService,
                                                    _locationService, _serviceService, _centerService, _promotionService,
                                                    vnpaySettings, _notificationService, _notificationAccountService, _staffService,
                                                    sendMailService, _cloudStorageService, _orderDetailTrackingService, _walletService,
                                                    _walletTransactionService, _paymentService, messageHub)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };
        }

        [Fact]
        public async Task UpdateOrder_ReturnOK()
        {
            //Arrange
            string orderId = "1";
            var fakeOrder = A.Fake<Order>();
            fakeOrder.Status = "confirmed"; // set status to not completed
            A.CallTo(() => _orderService.GetOrderById(A<string>.Ignored)).Returns(fakeOrder);
            //string staffId = "1";
            

            var fakeStaff = new Staff { CenterId = 1 };
            A.CallTo(() => _staffService.GetByAccountId(A<int>._)).Returns(fakeStaff);

            //Act
            var result = await _controller.UpdateOrderStatus(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

        }

        [Fact]
        public async Task CancelOrder_Should_Cancel_Order()
        {
            string orderId = "testOrderId";
            string cancelReason = "testCancelReason";
            
            var fakeOrder = A.Fake<Order>();
            fakeOrder.Status = "confirmed"; // set status to not completed
            A.CallTo(() => _orderService.GetOrderById(A<string>.Ignored)).Returns(fakeOrder);
            var fakeOrderService = A.Fake<IOrderService>();
            

            // Act
            var result = await _controller.CancelOrder(orderId, cancelReason);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.NotNull(result);
            Assert.Equal(0,response.StatusCode);
            Assert.Equal("success", response.Message);
        }

        [Fact]
        
        public async Task CompletedOrder_ReturnsOk_WhenOrderIsReadyAndPaid()
        {
            // Arrange
            var orderId = "123";
            var order = new Order
            {
                Id = orderId,
                Status = "Ready",
                DeliveryType = 1,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        PaymentMethod = 1,
                        Status = "Paid",
                        PlatformFee = 1,
                        WalletTransactions = new List<WalletTransaction>
                        {
                            new WalletTransaction
                            {
                                Type = "PayOrder",
                                Status = "Pending",
                                Amount = 10,
                                ToWalletId = 1,
                                UpdateTimeStamp = DateTime.Now
                            }
                        }
                    }
                }
            };
            A.CallTo(() => _orderService.GetOrderById(orderId)).Returns(order);
            A.CallTo(() => _paymentService.Update(A<Payment>._));
            A.CallTo(() => _orderService.Update(A<Order>._));

            // Act
            var result = await _controller.CompletedOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.NotNull(result);
            Assert.Equal(0, response.StatusCode);
            Assert.Equal("success", response.Message);
        }

        //[Fact]
        //public async Task UpdateOrderDetailStatus_WithValidData_ReturnsOk()
        //{
        //    // Arrange
        //    var orderId = "456";
        //    var orderDetailId = 456;
        //    var order = new Order
        //    {
        //        Id = orderId,
        //        Status = "Ready",
        //        DeliveryType = 1,
        //        OrderDetails = new List<OrderDetail>
        //        {
        //            new OrderDetail { Id = 456, Status = "pending" },
        //            new OrderDetail { Id = 789, Status = "shipped" }
        //        }
        //    };

        //    // Create a fake instance of _orderService
        //    var orderService = A.Fake<IOrderService>();

        //    // Configure the fake _orderService to return the fake order when GetOrderById is called with orderId
        //    A.CallTo(() => orderService.GetOrderById(orderId)).Returns(order);



        //    // Act
        //    var result = await _controller.UpdateOrderDetailStatus(orderId, orderDetailId);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.NotNull(result);
        //    Assert.Equal(0, response.StatusCode);
        //    Assert.Equal("success", response.Message);
        //}
    }
}
