using Microsoft.Extensions.Caching.Distributed;
using System;
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
using Washouse.Common.Mails;

namespace Washouse.xUnitTest.Controller
{
    public class VerifyControllerTests
    {
        private readonly ISMSService _smsService;
        private readonly IDistributedCache _cache;
        private readonly ISendMailService _sendMailService;
        
        public VerifyControllerTests()
        {
            _smsService = A.Fake<ISMSService>();
            _cache= A.Fake<IDistributedCache>();
            _sendMailService = A.Fake<ISendMailService>();  
        }

        //[Fact]
        //public void SendOTP_ReturnsOkResult()
        //{
        //    // Arrange
        //    var phoneNumber = "0975926021";
        //    var controller = new VerifyController(_smsService, _cache, _sendMailService);

        //    // Act
        //    var result = controller.SendOTP(phoneNumber);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        [Fact]
        public async Task SendMail_ReturnsOkResult()
        {
            // Arrange
            string testEmail = "test@example.com";
            //string expectedSubject = "Mã OTP Washouse";
            //string expectedContent = "Hello {recipient}, your OTP is: {otp}.";
            var controller = new VerifyController(_smsService, _cache, _sendMailService);

            // Act
            var result = await controller.SendMail(testEmail);

            // Assert
            var task = Task.FromResult<IActionResult>(result);
            var taskresponse = task.Result;
            var okResult = Assert.IsType<OkObjectResult>(taskresponse);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Equal("Send Successfully", response.Message);
            Assert.Null(response.Data);
        }

        //[Fact]
        //public void CheckOTP_ValidOTP_ReturnsOk()
        //{
        //    // Arrange
        //    string otp = "1234";

        //    var controller = new VerifyController(_smsService, _cache, _sendMailService);

        //    // Act
        //    var result =  controller.CheckOTP(otp);

        //    // Assert
        //    OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        //    ResponseModel responseModel = Assert.IsType<ResponseModel>(okResult.Value);
        //    Assert.Equal(StatusCodes.Status200OK, responseModel.StatusCode);
        //    Assert.Equal("Verify success", responseModel.Message);
        //    Assert.Equal(otp, responseModel.Data);
        //}
    }
}
