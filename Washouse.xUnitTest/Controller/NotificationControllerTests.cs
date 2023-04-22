using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Hubs;
using Washouse.Web.Models;


namespace Washouse.xUnitTest.Controller
{
    public class NotificationControllerTests
    {
        private readonly INotificationService _notificationService;
        private readonly IAccountService _accountService;
        private readonly INotificationAccountService _notificationAccountService;
        private readonly IHubContext<MessageHub> messageHub;

        public NotificationControllerTests()
        {
            _notificationService = A.Fake<INotificationService>();
            _accountService = A.Fake<IAccountService>();
            _notificationAccountService= A.Fake<INotificationAccountService>();
            messageHub = A.Fake<IHubContext<MessageHub>>();
        }

        [Fact]
        public void GetNotifications_WithValidToken_ReturnsOk()
        {
            var controller = new NotificationController(_notificationService,_accountService,_notificationAccountService,messageHub);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim("Id", "1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            // Act
            var result = controller.GetNotifications(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseModel = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(0, responseModel.StatusCode);
            Assert.Equal("success", responseModel.Message);
        }

        [Fact]
        public async Task ReadNotifications_WithValidToken_ReturnsOk()
        {
            var notiId = 1;
            var controller = new NotificationController(_notificationService, _accountService, _notificationAccountService, messageHub);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim("Id", "1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "User")
            }, "mock"));
            // Act
            var result =  await controller.UpdateNotification(notiId);

            // Assert
            var task = Task.FromResult<IActionResult>(result);
            var taskresponse = task.Result;
            var okResult = Assert.IsType<OkObjectResult>(taskresponse);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(0, response.StatusCode);
            Assert.Equal("success", response.Message);
        }
    }
}
