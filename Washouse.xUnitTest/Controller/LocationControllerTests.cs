using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class LocationControllerTests
    {
        private readonly ILocationService _locationService;
        private readonly IWardService _wardService;

        public LocationControllerTests()
        {
            _locationService = A.Fake<ILocationService>();
            _wardService = A.Fake<IWardService>();
        }

        [Fact]
        public async Task CreateLocation_ValidModel_ReturnsOk()
        {
            // Arrange
            var locationRequest = new LocationRequestModel
            {
                AddressString = "123 Main St",
                WardId = 1,
                Latitude = (decimal?)40.712776,
                Longitude = (decimal?)-74.005974
            };

            
            var controller = new LocationController(_locationService, _wardService);

            // Act
            var result = await controller.CreateLocation(locationRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(0, response.StatusCode);

            Assert.NotNull(response);
            Assert.Equal("success", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetById_ExistingLocation_ReturnsOk()
        {
            // Arrange
            var locationId = 1;
            var location = new Model.Models.Location
            {
                Id = locationId,
                AddressString = "123 Main St",
                WardId = 1,
                Latitude = (decimal?)40.712776,
                Longitude = (decimal?)-74.005974,
                Ward = new Model.Models.Ward
                {
                    Id = 1,
                    WardName = "Ward 1",
                    District = new Model.Models.District
                    {
                        Id = 1,
                        DistrictName = "District 1"
                    }
                }
            };
            var controller = new LocationController(_locationService, _wardService);
            // Act
            var result = await controller.GetById(locationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(0, response.StatusCode);

            Assert.NotNull(response);
            Assert.Equal("success", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public void Distance_ValidModel_ReturnsOk()
        {
            // Arrange
            decimal latitude1 = 37.7749M;
            decimal longitude1 = -122.4194M;
            decimal latitude2 = 40.7128M;
            decimal longitude2 = -74.0060M;

            var controller = new LocationController(_locationService, _wardService);

            // Act
            var result = controller.Distance(latitude1, longitude1, latitude2, longitude2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Equal("success", response.Message);
            Assert.NotNull(response.Data);
            
        }

        [Fact]
        public async Task Search_ValidAddress_ReturnsOk()
        {
            // Arrange
            
            string addressString = "123 Main St";
            int wardId = 1;
            var controller = new LocationController(_locationService, _wardService);
            // Act
            var result = await controller.search(addressString, wardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);

            Assert.NotNull(response);
            Assert.NotNull(response.Message);
            Assert.NotNull(response.Data);

            
        }

        [Fact]
        public async Task Time_ValidIds_ReturnsOk()
        {
            // Arrange
            var location1 = new Location { Id = 1, Latitude = (decimal?)40.712776, Longitude = (decimal?)-74.005974 };
            var location2 = new Location { Id = 2, Latitude = (decimal?)51.5074, Longitude = (decimal?)-0.1278 };
           
            var controller = new LocationController(_locationService, _wardService);
            

            // Act
            var result = await controller.Time(location1.Id, location2.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<int>(okResult.Value);
            //Assert.NotNull(response);
        }
    }
}
