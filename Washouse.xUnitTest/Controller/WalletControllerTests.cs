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
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    public class WalletControllerTests
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IWalletService _walletService;

        public WalletControllerTests()
        {
            _accountService = A.Fake<IAccountService>();
            _transactionService = A.Fake<ITransactionService>();
            _walletService = A.Fake<IWalletService>();
        }

        [Fact]
        public async Task ActiveWallet_ReturnsOkObjectResult()
        {
            // Arrange
            var userId = "1";
            var user = new Account
            {
                Id = 1,
                Email = "test@test.com",
                WalletId = null
            };
            
            var accountService = A.Fake<IAccountService>();
            var walletService = A.Fake<IWalletService>();
            var transactionService = A.Fake<ITransactionService>();
            var fakeUserIdentity = new ClaimsIdentity(new Claim[] { new Claim("Id", "1") });
            var fakeUser = new ClaimsPrincipal(fakeUserIdentity);
            A.CallTo(() => accountService.GetById(A<int>.Ignored)).Returns(user);
            A.CallTo(() => walletService.Add(A<Wallet>.Ignored));
            var controller = new WalletController(accountService, transactionService,walletService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = fakeUser }
                }
            };
            var wallet = new Wallet
            {
                //Id = 1,
                Status = "Active",
                CreatedDate = DateTime.Now,
                CreatedBy = user.Email,
                Balance = 0
            };
            // Act
            var result = await controller.ActiveWallet();


            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Equal("Success", response.Message);
            Assert.NotNull(response.Data);

            var walletResponse = Assert.IsType<Wallet>(response.Data);
            Assert.Equal(wallet.Id, walletResponse.Id);
            Assert.Equal(wallet.Status, walletResponse.Status);
            Assert.NotNull( walletResponse.CreatedDate);
            Assert.Equal(wallet.CreatedBy, walletResponse.CreatedBy);
            Assert.Equal(wallet.Balance, walletResponse.Balance);

            A.CallTo(() => accountService.GetById(int.Parse(userId))).MustHaveHappenedOnceExactly();
            A.CallTo(() => walletService.Add(A<Wallet>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => accountService.Update(user)).MustHaveHappenedOnceExactly();
        }
    }
}
