using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class FeedbackControllerTests
    {
        public readonly IFeedbackService _feedbackService;
        public readonly IAccountService _accountService;
        public readonly IOrderService _orderService;
        public readonly IServiceService _serviceService;
        public readonly ICustomerService _customerService;
        public readonly ICenterService _centerService;
        public readonly ICloudStorageService _cloudStorageService;

        public FeedbackControllerTests()
        {
            _feedbackService = A.Fake<IFeedbackService>();
            _accountService= A.Fake<IAccountService>();
            _orderService= A.Fake<IOrderService>();
            _serviceService= A.Fake<IServiceService>();
            _customerService= A.Fake<ICustomerService>();
            _centerService= A.Fake<ICenterService>();
            _cloudStorageService= A.Fake<ICloudStorageService>();
        }

        [Fact]
        public async Task FeedbackOrder_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
                      
            var model = new FeedbackOrderRequestModel
            {
                OrderId = "20230420_0000007",
                Content = "Test feedback",
                Rating = 5,
                CenterId = 1
            };
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };
            // Act
            var result = await controller.FeedbackOrder(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            //Assert.Equal("Model is not valid", response.Message);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task FeedbackOrder_ReturnsOKRequest_WhenModelStateIsValid()
        {
            // Arrange
            var fakeOrder = A.Fake<Order>();
            fakeOrder.Status = "Completed"; // set status to not completed
            A.CallTo(() => _orderService.GetOrderById(A<string>.Ignored)).Returns(fakeOrder);

            var model = new FeedbackOrderRequestModel
            {
                OrderId = "20230420_0000005",
                Content = "Test feedback",
                Rating = 5,
                CenterId = 3
            };
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim("Id", "1")
            }, "mock"));
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };
            // Act
            var result = await controller.FeedbackOrder(model);

            // Assert
            var badRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            //Assert.Equal("Model is not valid", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task FeedbackService_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange

            var model = new FeedbackServiceRequestModel
            {
                ServiceId = 12,
                Content = "Test feedback",
                Rating = 5,
                CenterId = 1
            };
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };
            // Act
            var result = await controller.FeedbackService(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            //Assert.Equal("Model is not valid", response.Message);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task FeedbackService_ReturnsOKRequest_WhenModelStateIsValid()
        {
            // Arrange
            var fakeOrder = A.Fake<Order>();
            fakeOrder.Status = "Completed"; // set status to not completed
            A.CallTo(() => _orderService.GetOrderById(A<string>.Ignored)).Returns(fakeOrder);

            var model = new FeedbackServiceRequestModel
            {
                ServiceId = 12,
                Content = "Test feedback",
                Rating = 5,
                CenterId = 1
            };
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim("Id", "1")
            }, "mock"));
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };
            // Act
            var result = await controller.FeedbackService(model);

            // Assert
            var badRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            //Assert.Equal("Model is not valid", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task ReplyFeedback_ReturnsOKRequest()
        {
            // Arrange
            var feedbackId = 1;
            var replyMessage = "Thank you for your feedback!";
            var fakeFeedbackService = A.Fake<IFeedbackService>();
            var existingFeedback = new Feedback { Id = feedbackId };
            A.CallTo(() => fakeFeedbackService.GetById(feedbackId)).Returns(existingFeedback);
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Manager"),
                new Claim("Id", "1")
            }, "mock"));
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };
            // Act
            var result = await controller.ReplyFeedback(new ReplyFeedbackRequestModel { ReplyMessage = replyMessage }, feedbackId) ;


            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
                     
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            
            Assert.NotNull(response);
            Assert.Equal("success", response.Message);
            Assert.NotNull(response.Data);
            //Assert.NotNull(result.);
            //Assert.Equal("test@example.com", existingFeedback.ReplyBy);
            //Assert.Equal(replyMessage, existingFeedback.ReplyMessage);
        }

        [Fact]
        public async Task GetFeedbackByCenterId_ReturnsFeedbackList()
        {
            // Arrange
            int centerId = 1;
            var filter = new PaginationViewModel { Page = 1, PageSize = 10 };
            
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService);

            // Act
            var result = await controller.GetFeedbackByCenterId(centerId, filter);

            // Assert
            var task = Task.FromResult<IActionResult>(result);
            var taskresponse = task.Result;
            var okResult = Assert.IsType<OkObjectResult>(taskresponse);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(0, response.StatusCode);
            Assert.Equal("success", response.Message);
            Assert.NotNull(response.Data);


        }

        [Fact]
        public async Task GetFeedbackByServiceId_ReturnsFeedbackList()
        {
            // Arrange
            int serviceid = 1;
            var filter = new PaginationViewModel { Page = 1, PageSize = 10 };
            
            var controller = new FeedbackController(_feedbackService, _accountService, _serviceService, _orderService, _customerService, _centerService, _cloudStorageService);

            // Act
            var result = await controller.GetAllByServiceId(serviceid, filter);

            // Assert
            var task = Task.FromResult<IActionResult>(result);
            var taskresponse = task.Result;
            var okResult = Assert.IsType<OkObjectResult>(taskresponse);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(0, response.StatusCode);
            Assert.Equal("success", response.Message);
            Assert.NotNull(response.Data);


        }
    }
}
