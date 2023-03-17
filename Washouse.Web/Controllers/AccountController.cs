using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data;
using Washouse.Model.Models;
using Washouse.Service;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly WashouseDbContext _context;
        private readonly AppSetting _appSettings;
        private readonly IAccountService _accountService;
        public AccountController(WashouseDbContext context, IOptionsMonitor<AppSetting> optionsMonitor, IAccountService accountService)
        {
            this._context = context;
            _appSettings = optionsMonitor.CurrentValue;
            this._accountService = accountService;
        }

        [HttpPost("login")]
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
            string token = GenerateToken(user);
            return Ok(new
            {
                Success = true,
                Message = "Authenticate success",
                Data = token
                //,
                //Email  = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token).Claims.First(claim => claim.Type.ToLower().Equals("Email".ToLower())).Value
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
                    new Claim(ClaimTypes.Role, nguoiDung.RoleType.Trim().ToString()),
                    new Claim("TokenId", Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);

            return jwtTokenHandler.WriteToken(token);
        }

        [Authorize(Roles ="Admin")]
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

        [HttpPost("addAccount")]
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

        [HttpPut("deactivateAccount/{id}")]
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

        [HttpPut("activateAccount/{id}")]
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

        [HttpPut("changePassword/{id}")]
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


        [HttpPut("resetPassword/{id}")]
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

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            string id = User.FindFirst("Id")?.Value;
            var user = _accountService.GetById(int.Parse(id));
            string token = GenerateToken(user.Result);
            return Ok(new
            {
                Success = true,
                Message = "success",
                Data = new
                {
                    TokenId = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token).Claims.First(claim => claim.Type.ToLower().Equals("TokenId".ToLower())).Value,
                    AccountId = int.Parse(id),
                    Email = User.FindFirst(ClaimTypes.Email)?.Value,
                    Phone = User.FindFirst("Phone")?.Value,
                    RoleType = User.FindFirst(ClaimTypes.Role)?.Value,
                    Name = User.FindFirst(ClaimTypes.Name)?.Value,
                    Avatar = user.Result.ProfilePic

                }
            });
        }
    }
}
