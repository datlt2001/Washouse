using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data;
using Washouse.Model.Models;
using Washouse.Service;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly WashouseDbContext _context;
        private readonly AppSetting _appSettings;
        private IAccountService _accountService;
        public AccountController(WashouseDbContext context, IOptionsMonitor<AppSetting> optionsMonitor, IAccountService accountService)
        {
            this._context = context;
            _appSettings = optionsMonitor.CurrentValue;
            this._accountService = accountService;
        }

        [HttpPost("Login")]
        public IActionResult Validate(LoginModel model)
        {
            //var user = _context.Accounts.SingleOrDefault(p =>
            //p.Phone == model.Phone && p.Password == model.Password);
            var user = _accountService.GetLoginAccount(model.Phone, model.Password);
            if (user == null)
            {
                return Ok(new
                {
                    Success = false,
                    Message = "Invalid username/password"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Authenticate success",
                Data = GenerateToken(user)
            });
        }

        private string GenerateToken(Account nguoiDung)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, nguoiDung.FullName),
                    new Claim(ClaimTypes.Email, nguoiDung.Email),
                    new Claim("Phone", nguoiDung.Phone),
                    new Claim("Id", nguoiDung.Id.ToString()),

                    //roles

                    new Claim("TokenId", Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);

            return jwtTokenHandler.WriteToken(token);
        }

        [HttpGet]
        public IActionResult GetAccountList()
        {
            var accounts = _accountService.GetAll();
            if (accounts == null) { return NotFound(); }
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var accounts = await _accountService.GetById(id);
            if (accounts == null) { return NotFound(); }
            return Ok(accounts);
        }

        [HttpPost("AddAccount")]
        public IActionResult Create(Account account)
        {
            if (ModelState.IsValid)
            {
                account.Id = 0;
                account.CreatedDate = DateTime.Now;
                account.UpdatedDate = DateTime.Now;
                account.Status = false;
                var accounts = _accountService.Add(account);
                return Ok(accounts);
            }
            else { return BadRequest(); }

        }

        [HttpPut("DeactivateAccount/{id}")]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            await _accountService.DeactivateAccount(id);
            return Ok();
        }

        [HttpPut("ActivateAccount/{id}")]
        public async Task<IActionResult> ActivateAccount(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            await _accountService.ActivateAccount(id);
            return Ok();
        }

        [HttpPut("ChangePassword/{id}")]
        public async Task<IActionResult> ChangePassword(int id,string oldPass,string newPass)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                if(account.Password != oldPass) {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Wrong password"
                    });
                }
                else
                {

                    await _accountService.ChangePassword(id, newPass);
                    return Ok(new
                    {
                        Success = true,
                        Message = "Your password has been changed"
                    });
                }
                
            }           
        }


        [HttpPut("ResetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                string newPass = GenerateRandomString(10);
                await _accountService.ChangePassword(id, newPass);
                return Ok(new
                {
                    Success = true,
                    Message = "Your password has been reset"
                });              

            }
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }
    }
}
