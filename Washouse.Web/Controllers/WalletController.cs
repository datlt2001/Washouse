using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Utilities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/wallets")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private IAccountService _accountService;
        private ITransactionService _transactionService;
        private IWalletService _walletService;
        private ICenterService _centerService;

        public WalletController(IAccountService accountService, ITransactionService transactionService, IWalletService walletService, ICenterService centerService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _walletService = walletService;
            _centerService = centerService;
        }

        [Authorize(Roles = "Customer, Manager")]
        [HttpPost("active")]
        public async Task<IActionResult> ActiveWallet()
        {
            string userId = User.FindFirst("Id")?.Value;
            string role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            Wallet wallet = new Wallet();
            wallet.Status = "Active";
            wallet.CreatedDate = DateTime.Now;
            wallet.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
            wallet.Balance = 0;
            await _walletService.Add(wallet);
            if (role.Trim().ToLower().Equals("customer"))
            {
                Account user = await _accountService.GetByIdLightWeight(int.Parse(userId));
                user.WalletId = wallet.Id;
                await _accountService.Update(user);
            }
            else
            {
                int centerid = int.Parse(User.FindFirst("CenterManaged")?.Value);
                var center = await _centerService.GetByIdLightWeight(centerid);
                center.HasOnlinePayment = true;
                center.WalletId = wallet.Id;
                await _centerService.Update(center);
            }
            
           
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = wallet
            });
        }
    }
}
