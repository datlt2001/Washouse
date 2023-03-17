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
using Washouse.Common.Helpers;
using Washouse.Common.Mails;
using Washouse.Data;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
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
        private IAccountService _accountService;
        private ISendMailService _sendMailService;
        private ICustomerService _customerService;
        public IStaffService _staffService;
        public AccountController(WashouseDbContext context, IOptionsMonitor<AppSetting> optionsMonitor, 
            IAccountService accountService, ISendMailService sendMailService, ICustomerService customerService, IStaffService staffService)
        {
            this._context = context;
            _appSettings = optionsMonitor.CurrentValue;
            this._accountService = accountService;
            this._sendMailService = sendMailService;
            _customerService = customerService;
            _staffService = staffService;
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

        [HttpPost("addStaffByManager")]
        public async Task<IActionResult> Create([FromForm] StaffRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var accounts = new Account()
                {
                    Phone = Input.Phone,
                    Email = Input.Email,
                    Password = Input.Password,
                    FullName = Input.FullName,
                    Dob = Input.Dob,
                    Status = false,
                    RoleType = "Staff",
                    //ProfilePic = await Utilities.UploadFile(Input.profilePic, @"images\accounts\staffs", Input.profilePic.FileName),
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.FullName,

                };
                await _accountService.Add(accounts);
                var staff = new Staff()
                {
                    AccountId = accounts.Id,
                    Status = false,
                    IsManager = true,
                    IdNumber = Input.IdNumber,
                    //IdFrontImg = await Utilities.UploadFile(Input.IdFrontImg, @"images\accounts\staffs", Input.profilePic.FileName),
                    //IdBackImg = await Utilities.UploadFile(Input.IdBackImg, @"images\accounts\staffs", Input.profilePic.FileName),
                    CreatedBy = accounts.CreatedBy,
                    CreatedDate = DateTime.Now,
                };
                await _staffService.Add(staff);
                return Ok(accounts);
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
                string newPass = Utilities.GenerateRandomString(10);
                await _accountService.ChangePassword(id, newPass);
                string path = "./Templates_email/ResetPassword.txt";
                string content = System.IO.File.ReadAllText(path);
                content = content.Replace("{recipient}", account.FullName);
                content = content.Replace("{resetpass}", newPass);
                await _sendMailService.SendEmailAsync("minhkilo64@gmail.com", "Reset Password", content);
                return Ok(new
                {
                    Success = true,
                    Message = "Your password has been reset"
                });              

            }
        }

        [HttpPut("forgotPassword/{id}")]
        public async Task<IActionResult> ForgotPassword(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                string newPass = Utilities.GenerateRandomString(10);
                await _accountService.ChangePassword(id, newPass);
                string path = "./Templates_email/ForgotPassword.txt";
                string content = System.IO.File.ReadAllText(path);
                content = content.Replace("{recipient}", account.FullName);
                content = content.Replace("{account}", account.Phone);
                content = content.Replace("{resetpass}", newPass);
                await _sendMailService.SendEmailAsync("minhkilo64@gmail.com", "Forgot Password", content);
                return Ok(new
                {
                    Success = true,
                    Message = "Your password has been reset"
                });

            }
        }

        [HttpPost("RegisterAsCustomer")]
        public async Task<IActionResult> RegisterAsCustomer([FromForm] CustomerRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var accounts = new Account()
                {
                    Phone = Input.Phone,
                    Email = Input.Email,
                    Password = Input.Password,
                    FullName = Input.FullName,
                    Dob = Input.Dob,
                    Status = false,
                    RoleType = "Customer",
                    //ProfilePic = await Utilities.UploadFile(Input.profilePic, @"images\accounts\customer", Input.profilePic.FileName),
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.FullName,                   

                };
                await _accountService.Add(accounts);
                var customer = new Customer()
                {
                    AccountId= accounts.Id,
                    Fullname= accounts.FullName,
                    Phone = accounts.Phone,
                    Email = accounts.Email,
                    Status = false,
                    CreatedBy= accounts.CreatedBy, 
                    CreatedDate= DateTime.Now,
                };
                await _customerService.Add(customer);
                return Ok(accounts);
            }
            else { return BadRequest(); }

        }

        [HttpPost("RegisterAsManager")]
        public async Task<IActionResult> RegisterAsManager([FromForm] StaffRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var accounts = new Account()
                {
                    Phone = Input.Phone,
                    Email = Input.Email,
                    Password = Input.Password,
                    FullName = Input.FullName,
                    Dob = Input.Dob,
                    Status = false,
                    RoleType = "Manager",
                    //ProfilePic = await Utilities.UploadFile(Input.profilePic, @"images\accounts\managers", Input.profilePic.FileName),
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.FullName,

                };
                await _accountService.Add(accounts);
                var manager = new Staff()
                {
                    AccountId = accounts.Id,                   
                    Status = false,
                    IsManager = true,
                    CenterId = null,
                    IdNumber = Input.IdNumber,
                    //IdFrontImg = await Utilities.UploadFile(Input.IdFrontImg, @"images\accounts\managers", Input.profilePic.FileName),
                    //IdBackImg = await Utilities.UploadFile(Input.IdBackImg, @"images\accounts\managers", Input.profilePic.FileName),
                    CreatedBy = accounts.CreatedBy,
                    CreatedDate = DateTime.Now,
                };
                await _staffService.Add(manager);
                return Ok(accounts);
            }
            else { return BadRequest(); }

        }

        [HttpPut("veriyAccount/{id}")]
        public async Task<IActionResult> VerifyAccount(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            await _accountService.ActivateAccount(id);
            return Ok();
        }

    }
}
