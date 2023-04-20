using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class ServiceCategoryControllerTests
    {
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly IServiceService _serviceService;
        private readonly ICloudStorageService _cloudStorageService;
        public ServiceCategoryControllerTests() 
        {
            _serviceCategoryService = A.Fake<IServiceCategoryService>();
            _serviceService = A.Fake<IServiceService>();
            _cloudStorageService = A.Fake<ICloudStorageService>();
        }

        [Fact]
        public async Task GetCategoryList_ReturnsOkObjectResult()
        {
            // Arrange
            var categories = A.CollectionOfDummy<Category>(2);
            var categoryList = categories.ToList();
            var serviceCategoryService = A.Fake<IServiceCategoryService>();
            A.CallTo(() => serviceCategoryService.GetAll()).Returns(categoryList);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            var serviceService = A.Fake<IServiceService>();
            var controller = new ServiceCategoryController(serviceCategoryService,serviceService,cloudStorageService);

            // Act
            var result = await controller.GetCategoryList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(0, response.StatusCode);
            Assert.NotNull(response.Data);



        }

        [Fact]
        public async Task GetCategoryById_WithValidId_ReturnsOkObjectResult()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                Alias = "test-category",
                Description = "Test Description",
                HomeFlag = true,
                Image = "test.jpg"
            };
            var serviceCategoryService = A.Fake<IServiceCategoryService>();
            A.CallTo(() => serviceCategoryService.GetById(categoryId)).Returns(category);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            var serviceService = A.Fake<IServiceService>();
            var controller = new ServiceCategoryController(serviceCategoryService, serviceService, cloudStorageService);

            // Act
            var result = await controller.GetCategoryById(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(0, response.StatusCode);
            Assert.NotNull(response.Data);

            var categoryResponse = Assert.IsType<CategoryResponseModel>(response.Data);
            Assert.Equal(categoryId, categoryResponse.CategoryId);
            Assert.Equal(category.CategoryName, categoryResponse.CategoryName);
            Assert.Equal(category.Alias, categoryResponse.CategoryAlias);
            Assert.Equal(category.Description, categoryResponse.Description);
            Assert.Equal(category.HomeFlag, categoryResponse.HomeFlag);
            Assert.NotNull(categoryResponse.Image);
        }

        [Fact]
        public async Task GetCategoryById_WithInvalidId_ReturnsNotFoundObjectResult()
        {
            // Arrange
            int categoryId = 1;
            Category category = null;
            var serviceCategoryService = A.Fake<IServiceCategoryService>();
            A.CallTo(() => serviceCategoryService.GetById(categoryId)).Returns(category);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            var serviceService = A.Fake<IServiceService>();
            var controller = new ServiceCategoryController(serviceCategoryService, serviceService, cloudStorageService);

            // Act
            var result = await controller.GetCategoryById(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(notFoundResult.Value);

            Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
            Assert.Equal("Not found category.", response.Message);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task Create_WithValidInput_ReturnsOkObjectResult()
        {
            // Arrange
            var input = new CreateCategoryRequestModel
            {
                CategoryName = "Test Category",
                Description = "Test Description",
                HomeFlag = true,
                SavedFileName = "test.jpg",
                Status = true
            };
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));
            var fakeServiceCategoryService = A.Fake<IServiceCategoryService>();
            var fakeCloudStorageService = A.Fake<ICloudStorageService>();
            var serviceService = A.Fake<IServiceService>();
            var controller = new ServiceCategoryController(fakeServiceCategoryService, serviceService, fakeCloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };


            // Act
            var result = await controller.Create(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(0, response.StatusCode);
            Assert.Equal("success", response.Message);

            var categoryResponse = Assert.IsType<CategoryResponseModel>(response.Data);
            Assert.NotNull(categoryResponse.CategoryId);
            Assert.Equal(input.CategoryName, categoryResponse.CategoryName);
            //Assert.NotNull(categoryResponse.CategoryAlias);
            Assert.Equal(input.Description, categoryResponse.Description);
            Assert.Equal(input.HomeFlag, categoryResponse.HomeFlag);
            Assert.NotNull(categoryResponse.Image);
        }

        [Fact]
        public async Task UpdateCategory_WithValidCategory_ReturnsOkObjectResult()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                Alias = "test-category",
                Description = "Test Description",
                HomeFlag = true,
                Image = "test.jpg"
            };
            var updatedCategory = new CategoryRequestModel
            {
                CategoryName = "Updated Category",
                Alias = "updated-category",
                Description = "Updated Description",
                HomeFlag = false,
                SavedFileName = "updated.jpg"
            };
            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));
            var serviceCategoryService = A.Fake<IServiceCategoryService>();
            A.CallTo(() => serviceCategoryService.GetById(categoryId)).Returns(category);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            var serviceService = A.Fake<IServiceService>();
            var controller = new ServiceCategoryController(serviceCategoryService, serviceService, cloudStorageService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeUser }
                }
            };

            // Act
            var result = await controller.UpdateCategory(updatedCategory, categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(0, response.StatusCode);
            Assert.NotNull(response.Data);

            var categoryResponse = Assert.IsType<CategoryResponseModel>(response.Data);
            Assert.Equal(categoryId, categoryResponse.CategoryId);
            Assert.Equal(updatedCategory.CategoryName, categoryResponse.CategoryName);
            //Assert.Equal(updatedCategory.Alias, categoryResponse.CategoryAlias);
            Assert.Equal(updatedCategory.Description, categoryResponse.Description);
            Assert.Equal(updatedCategory.HomeFlag, categoryResponse.HomeFlag);
            Assert.NotNull(categoryResponse.Image);
        }

    }
}
