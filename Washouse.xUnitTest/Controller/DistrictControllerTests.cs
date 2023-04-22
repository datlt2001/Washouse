using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class DistrictControllerTests
    {
        private readonly IDistrictService _districtService;
        private readonly IWardService _wardService;

        public DistrictControllerTests()
        {
            _districtService = A.Fake<IDistrictService>();
            _wardService= A.Fake<IWardService>();
        }

        [Fact]
        public async Task GetAll_ReturnsListOfDistricts()
        {
            // Arrange
            var fakeDistrict1 = new District { Id = 1, DistrictName = "District 1" };
            var fakeDistrict2 = new District { Id = 2, DistrictName = "District 2" };
            var fakeDistrictList = new List<District> { fakeDistrict1, fakeDistrict2 };

            var fakeDistrictService = A.Fake<IDistrictService>();
            var fakeWardService = A.Fake<IWardService>();
            A.CallTo(() => fakeDistrictService.GetAll()).Returns(fakeDistrictList);

            var controller = new DistrictController(fakeDistrictService, fakeWardService);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetDistrictByLatLong_ReturnsBadRequest_WhenResponseIsNotSuccessful()
        {
            // Arrange
            double latitude = 10.1234;
            double longitude = 20.5678;
            //var httpClient = A.Fake<HttpClient>();
            //A.CallTo(() => httpClient.GetAsync(A<string>.Ignored)).Returns(new HttpResponseMessage
            //{
            //    StatusCode = HttpStatusCode.BadRequest,
            //    ReasonPhrase = "Bad Request"
            //});
            var controller = new DistrictController(A.Fake<IDistrictService>(), A.Fake<IWardService>());

            // Act
            var result = await controller.GetDistrictByLatLong(latitude, longitude);

            // Assert
            var okResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            
            Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
            Assert.NotNull(response.Message);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task GetDistrictByLatLong_ReturnsBadRequest_WhenResponseIsSuccessful()
        {
            // Arrange
            double latitude = 10.837932276920075;
            double longitude = 106.67161398782503;
            var httpClient = A.Fake<HttpClient>();
            var responsefake = new HttpResponseMessage(HttpStatusCode.OK);
            responsefake.Content = new StringContent(@"{
              ""address"": {
                ""city_district"": ""Quận 1"",
                ""city"": ""Thành phố Hồ Chí Minh""
              }
            }");
            var district = new District { Id = 1, DistrictName = "Quận 1" };
            A.CallTo(() => _districtService.GetDistrictByName("Quận 1")).Returns(district);

            var controller = new DistrictController(A.Fake<IDistrictService>(), A.Fake<IWardService>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        RequestServices = A.Fake<IServiceProvider>()
                    }
                }
            };

            // Act
            var result = await controller.GetDistrictByLatLong(latitude, longitude);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.NotNull(response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetWardList()
        {
            int districtId = 1;
            List<Ward> wardList = new List<Ward>
            {
                new Ward { Id = 1, WardName = "Ward 1" },
                new Ward { Id = 2, WardName = "Ward 2" }
            };
            A.CallTo(() => _wardService.GetWardListByDistrictId(districtId)).Returns(wardList);
            var controller = new DistrictController(_districtService,_wardService);

            // Act
            var result = await controller.GetWardListByDistrict(districtId);

            // Assert
            A.CallTo(() => _wardService.GetWardListByDistrictId(districtId)).MustHaveHappenedOnceExactly();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Equal("success", response.Message);
            var responseData = (List<WardResponseModel>)response.Data;
            Assert.Equal(wardList.Count, responseData.Count);
            for (int i = 0; i < wardList.Count; i++)
            {
                Assert.Equal(wardList[i].Id, responseData[i].WardId);
                Assert.Equal(wardList[i].WardName, responseData[i].WardName);
            }
        }
    }
}
