using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class CustomerControllerTests
    {
        private readonly ICustomerService _customerService;
        public IAccountService _accountService;
        public IWardService _wardService;
        public ILocationService _locationService;
        public ICloudStorageService _cloudStorageService;
        public IDistrictService _districtService;
        private CustomerController _controller;

        public CustomerControllerTests()
        {
            _customerService = A.Fake<ICustomerService>();
            _accountService = A.Fake<IAccountService>();
            _wardService= A.Fake<IWardService>();
            _locationService= A.Fake<ILocationService>();
            _cloudStorageService= A.Fake<ICloudStorageService>();
            _districtService= A.Fake<IDistrictService>();

            _controller = new CustomerController(_customerService, _accountService, _wardService,
                                                _locationService, _cloudStorageService, _districtService);
        }

        [Fact]
        public void GetCustomerList_ReturnsCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Fullname = "John Doe" },
                new Customer { Id = 2, Fullname = "Jane Smith" }
            };

            var fakeCustomerService = A.Fake<ICustomerService>();
            A.CallTo(() => fakeCustomerService.GetAll()).Returns(customers);

            

            // Act
            var result = _controller.GetCustomerList() as OkObjectResult;

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.NotNull(response);
            Assert.Equal("Success", response.Message);

            
        }

        [Fact]
        public async Task GetCustomerById_ReturnsOk_WhenCustomerExists()
        {
            // Arrange
            int customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                Fullname = "John Doe",
                Phone = "123456789",
                Email = "john.doe@example.com",
                AccountId = 2,
                Address = 3
            };
            var account = new Account
            {
                Id = customer.AccountId.Value,
                Gender = 1,
                Dob = new DateTime(1990, 1, 1),
                WalletId = 4,
                ProfilePic = "profile-pic.jpg"
            };
            var location = new Location
            {
                Id = customer.Address.Value,
                AddressString = "123 Main St",
                WardId = 5,
                Latitude = 10.1234M,
                Longitude = 20.5678M
            };
            var ward = new Ward
            {
                Id = location.WardId,
                WardName = "Ward 1",
                DistrictId = 6
            };
            var district = new District
            {
                Id = ward.DistrictId,
                DistrictName = "District 1"
            };
            var customerService = A.Fake<ICustomerService>();
            A.CallTo(() => customerService.GetById(customerId)).Returns(customer);

            var accountService = A.Fake<IAccountService>();
            A.CallTo(() => accountService.GetById(customer.AccountId.Value)).Returns(account);

            var locationService = A.Fake<ILocationService>();
            A.CallTo(() => locationService.GetById(customer.Address.Value)).Returns(location);

            var wardService = A.Fake<IWardService>();
            A.CallTo(() => wardService.GetWardById(location.WardId)).Returns(ward);

            var districtService = A.Fake<IDistrictService>();
            A.CallTo(() => districtService.GetDistrictById(ward.DistrictId)).Returns(district);
            // Act

            var result = await _controller.GetCustomerById(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerByAccId_ReturnsOk_WhenCustomerExists()
        {
            // Arrange
            int accountId = 123;
            int customerId = 456;
            var user = new Account { Id = accountId, Gender = 1, WalletId = 1 };
            var customer = new Customer { Id = customerId, Fullname = "John Doe", Email = "johndoe@example.com", Phone = "123456789" };
            customer.Address = 1; // set a non-null value for testing purposes
            var location = new Location{ Id = 1, AddressString = "123 Main St", Latitude = 1.0M, Longitude = 1.0M, WardId = 2 };
            var ward = new Ward { Id = 2, WardName = "Ward 2", DistrictId = 3 };
            var district = new District { Id = 3, DistrictName = "District 3" };
            A.CallTo(() => _accountService.GetById(accountId)).Returns(user);
            A.CallTo(() => _customerService.GetCustomerByAccID(user.Id)).Returns(customer);
            A.CallTo(() => _locationService.GetById(location.Id)).Returns(location);
            A.CallTo(() => _wardService.GetWardById(ward.Id)).Returns(ward);
            A.CallTo(() => _districtService.GetDistrictById(district.Id)).Returns(district);
            //A.CallTo(() => _cloudStorageService.GetSignedUrlAsync(user.ProfilePic)).Returns("https://example.com/profile.jpg");

            var expectedResponse = new CustomerDetailResponseModel
            {
                Id = customerId,
                AccountId = accountId,
                Fullname = customer.Fullname,
                Email = customer.Email,
                Phone = customer.Phone,
                WalletId = user.WalletId,
                Gender = user.Gender,
                Dob = null, // user.Dob is null in this test
                AddressString = "123 Main St, Ward 2, District 3, Thành Phố Hồ Chí Minh",
                Address = new CustomerLocatonResponseModel
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    AddressString = location.AddressString,
                    Ward = new WardResponseModel
                    {
                        WardId = ward.Id,
                        WardName = ward.WardName,
                        District = new DistrictResponseModel
                        {
                            DistrictId = district.Id,
                            DistrictName = district.DistrictName
                        }
                    }
                },
                ProfilePic = "https://example.com/profile.jpg"
            };

            // Act

            var result = await _controller.GetCustomerByAccountId(accountId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
        }
    }
}
