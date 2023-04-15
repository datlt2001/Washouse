using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Utilities;
using System;
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

        public WalletController(IAccountService accountService, ITransactionService transactionService, IWalletService walletService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _walletService = walletService;
        }

        [HttpPost("active")]
        public async Task<IActionResult> ActiveWallet()
        {
            string userId = User.FindFirst("Id")?.Value;
            Account user = await _accountService.GetById(int.Parse(userId));
            Wallet wallet = new Wallet();
            wallet.Status = "Active";
            wallet.CreatedDate = DateTime.Now;
            wallet.CreatedBy = user.Email;
            wallet.Balance = 0;
            await _walletService.Add(wallet);
            user.WalletId = wallet.Id;
            await _accountService.Update(user);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = wallet
            });
        }
    }
}
