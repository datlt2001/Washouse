using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Common.Mails;
using Washouse.Data;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Service;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly WashouseDbContext _context;
        private readonly AppSetting _appSettings;
        private IAccountService _accountService;
        private ISendMailService _sendMailService;
        private ICustomerService _customerService;
        public IStaffService _staffService;
        public ICloudStorageService _cloudStorageService;
        public IWalletService _walletService;
        public IWardService _wardService;
        public ILocationService _locationService;
        public AccountController(WashouseDbContext context, IOptionsMonitor<AppSetting> optionsMonitor, 
            IAccountService accountService, ISendMailService sendMailService, ICustomerService customerService,
            IStaffService staffService, ICloudStorageService cloudStorageService, IWalletService walletService, IWardService wardService, ILocationService locationService)
        {
            this._context = context;
            _appSettings = optionsMonitor.CurrentValue;
            this._accountService = accountService;
            this._sendMailService = sendMailService;
            this._customerService = customerService;
            this._staffService = staffService;
            this._cloudStorageService = cloudStorageService;
            this._walletService = walletService;
            _wardService = wardService;
            _locationService = locationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validate(LoginModel model)
        {
            //var user = _context.Accounts.SingleOrDefault(p =>
            //p.Phone == model.Phone && p.Password == model.Password);
            var user = _accountService.GetLoginAccount(model.Phone, model.Password);
            if (user == null)
            {
                return Ok(new ResponseModel
                {
                    StatusCode = 10,
                    Message = "Invalid phone or password",
                    Data = null
                });
            }
            var token = await GenerateToken(user, false);
            if (user.IsAdmin) {
                return Ok(new ResponseModel
                {
                    StatusCode = 17,
                    Message = "Success auth admin",
                    Data = token
                });
            }
            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "Success",
                Data = token
            });
        }

        [HttpPost("login-staff")]
        public async Task<IActionResult> ValidateStaff(LoginModel model)
        {
            //var user = _context.Accounts.SingleOrDefault(p =>
            //p.Phone == model.Phone && p.Password == model.Password);
            var user = _accountService.GetLoginAccount(model.Phone, model.Password);
            if (user == null)
            {
                return Ok(new ResponseModel
                {
                    StatusCode = 10,
                    Message = "Invalid phone or password",
                    Data = null
                });
            }

            if (user.IsAdmin)
            {
                var _token = await GenerateToken(user, true);
                return Ok(new ResponseModel
                {
                    StatusCode = 17,
                    Message = "Success auth admin",
                    Data = _token
                });
            }

            var staff = await _staffService.GetByAccountId(user.Id);
            if (staff == null)
            {
                var tokenUser = await GenerateToken(user, true);
                return Ok(new ResponseModel
                {
                    StatusCode = 77,
                    Message = "Missing user information",
                    Data = tokenUser
                });
            }
            var token = await GenerateToken(user, true);
            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "Success",
                Data = token
            });
        }

        private async Task<TokenModel> GenerateToken(Account user, bool isManage)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
            string Role = null;
            var staff = await _staffService.GetByAccountId(user.Id);
            var customer = await _customerService.GetCustomerByAccID(user.Id);
            int centerManaged = 0;
            //var customer = _customerService.GetCustomerByAccID(user.Id);
            if (user.IsAdmin)
            {
                Role = "Admin";
            } else if (!isManage)
            {
                if (customer == null)
                {
                    try {
                        var cusAdding = new Customer()
                        {
                            AccountId = user.Id,
                            Status = true,
                            Fullname = user.FullName,
                            Phone = user.Phone,
                            Address = user.LocationId,
                            Email = user.Email,
                            CreatedDate = DateTime.Now,
                            CreatedBy = "AutoInserted"
                        };
                        await _customerService.Add(cusAdding);
                    } catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                Role = "Customer";
            } else
            {
                if (staff != null)
                {
                    if (staff.IsManager != null && staff.IsManager == true)
                    {
                        Role = "Manager";
                        centerManaged = (staff.CenterId != null ? (int)staff.CenterId : 0);
                    }
                    else if (staff.IsManager != null && staff.IsManager == false)
                    {
                        Role = "Staff";
                    }
                } else if (staff == null)
                {
                    Role = "User";
                }
            }

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Phone", user.Phone),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    //roles
                    new Claim(ClaimTypes.Role, Role),
                    new Claim("CenterManaged", centerManaged.ToString()),
                    new Claim("TokenId", Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                JwtId = token.Id,
                AccountId = user.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(1)
            };

            await _context.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }

        [HttpPost("token")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
            var tokenValidateParam = new TokenValidationParameters
            {
                //tự cấp token
                ValidateIssuer = false,
                ValidateAudience = false,

                //ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero,

                ValidateLifetime = false //ko kiểm tra token hết hạn
            };
            try
            {
                //check 1: AccessToken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)//false
                    {
                        return Ok(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Invalid token",
                            Data = null
                        });
                    }
                }

                //check 3: Check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Access token has not yet expired",
                        Data = null
                    });
                }

                //check 4: Check refreshtoken exist in DB
                var storedToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                if (storedToken == null)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Refresh token does not exist",
                        Data = null
                    });

                }

                //check 5: check refreshToken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Refresh token has been used",
                        Data = null
                    });
                }
                if (storedToken.IsRevoked)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Refresh token has been revoked",
                        Data = null
                    });
                }

                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Token doesn't match",
                        Data = null
                    });
                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _context.Update(storedToken);
                await _context.SaveChangesAsync();

                //create new token
                var user = await _context.Accounts.SingleOrDefaultAsync(nd => nd.Id == storedToken.AccountId);
                var token = new TokenModel();
                if (User.FindFirst(ClaimTypes.Role)?.Value == "Customer")
                {
                    token = await GenerateToken(user, false);
                } else if (User.FindFirst(ClaimTypes.Role)?.Value != "Customer") {
                    token = await GenerateToken(user, true);
                }

                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Renew token success",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();

            return dateTimeInterval;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAccountList()
        {
            var accounts = _accountService.GetAll();
            if (accounts == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found account",
                    Data = null
                });
            }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = accounts
            });
        }
            
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var accounts = await _accountService.GetById(id);
            if (accounts == null) {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found account",
                    Data = null
                });
            }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = accounts
            });
        }

        [HttpPost("staffs")]
        public async Task<IActionResult> Create([FromBody] CreateStaffRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var existphone = _accountService.GetAccountByPhone(Input.Phone);
                var existemail = _accountService.GetAccountByEmail(Input.Email);
                if(existphone != null || existemail != null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Email or Phone have existed please change",
                        Data = null
                    });
                }

                var accounts = new Account()
                {
                    Phone = Input.Phone,
                    Email = Input.Email,
                    Password = Input.Password,
                    FullName = Input.FullName,                    
                    Status = false,
                    IsAdmin = false,
                    //ProfilePic = await Utilities.UploadFile(Input.profilePic, @"images\accounts\staffs", Input.profilePic.FileName),
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.FullName,

                };
                await _accountService.Add(accounts);
                var staff = new Staff()
                {
                    AccountId = accounts.Id,
                    Status = false,
                    IsManager = false,
                    IdNumber = Input.IdNumber,
                    //IdFrontImg = await Utilities.UploadFile(Input.IdFrontImg, @"images\accounts\staffs", Input.profilePic.FileName),
                    //IdBackImg = await Utilities.UploadFile(Input.IdBackImg, @"images\accounts\staffs", Input.profilePic.FileName),
                    CreatedBy = accounts.CreatedBy,
                    CreatedDate = DateTime.Now,
                };
                await _staffService.Add(staff);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Updated",
                    Data = staff
                });

            }
            else {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Model is not valid",
                    Data = null
                });
            }
        }


        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateAccount(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found account",
                    Data = null
                });
            }
            await _accountService.DeactivateAccount(id);
            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "sucess",
                Data = new
                {
                    AccountId = id
                }
            });
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateAccount(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found account",
                    Data = null
                });
            }
            await _accountService.ActivateAccount(id);
            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "sucess",
                Data = new
                {
                    AccountId = id
                }
            });
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody]ChangePasswordViewModel changePasswordModel)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                if(account.Password != changePasswordModel.oldPass) {
                    return Ok(new ResponseModel
                    {
                        StatusCode = 200,
                        Message = "Wrong password",
                        Data = null
                    });
                }
                else
                {

                    await _accountService.ChangePassword(id, changePasswordModel.newPass);
                    return Ok(new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = new
                        {
                            AccountId = id
                        }
                    });
                }
                
            }           
        }

        [HttpPut("{email}/change-password-by-email")]
        public async Task<IActionResult> ChangePasswordByEmail(string email, [FromBody] ChangePasswordViewModel changePasswordModel)
        {
            Account account =  _accountService.GetAccountByEmail(email);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                if (account.Password != changePasswordModel.oldPass)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = 200,
                        Message = "Wrong password",
                        Data = null
                    });
                }
                else
                {

                    await _accountService.ChangePassword(account.Id, changePasswordModel.newPass);
                    return Ok(new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = new
                        {
                            AccountId = account.Id
                        }
                    });
                }

            }
        }


        [HttpPut("{id}/password/reset")]
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

        [HttpPut("{id}/password/forgot")]
        public async Task<IActionResult> ForgotPassword(int id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                //string newPass = Utilities.GenerateRandomString(10);
                //await _accountService.ChangePassword(id, newPass);
                string path = "./Templates_email/ForgotPassword.txt";
                string content = System.IO.File.ReadAllText(path);
                Random random = new Random();
                string otp = random.Next(1000, 9999).ToString();
                content = content.Replace("{recipient}", account.FullName);
                content = content.Replace("{otp}", otp);
                //content = content.Replace("{resetpass}", newPass);
                await _sendMailService.SendEmailAsync(account.Email, "Forgot Password", content);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Updated",
                    Data = otp
                });

            }
        }

        [HttpPost("customers")]
        public async Task<IActionResult> RegisterAsCustomer([FromBody] AccountRegisRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var existphone = _accountService.GetAccountByPhone(Input.Phone);
                var existemail = _accountService.GetAccountByEmail(Input.Email);
                if (existphone != null || existemail != null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Email or Phone have existed please change",
                        Data = null
                    });
                }
                var accounts = new Account()
                {
                    Phone = Input.Phone,
                    Email = Input.Email,
                    Password = Input.Password,
                    Status = false,
                    FullName = Input.Phone,
                    //RoleType = "Customer",
                    //ProfilePic = await Utilities.UploadFile(Input.profilePic, @"images\accounts\customer", Input.profilePic.FileName),
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.Email,

                };
                if (Input.confirmPass != Input.Password) { 
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Sai ConfirmPassword",
                        Data = null
                    }); }
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
                //string path = "./Templates_email/VerifyAccount.txt";
                //string content = System.IO.File.ReadAllText(path);
                //content = content.Replace("{recipient}", customer.Fullname);
                //string url = "https://localhost:44360/api/accounts/veriyAccount/"+ customer.AccountId;
                //content = content.Replace("{link}", url);
                //await _sendMailService.SendEmailAsync("minhkilo64@gmail.com", "Verify Account", content);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = accounts
                });
            }
            else { return BadRequest(); }

        }

        [HttpPost("managers")]
        public async Task<IActionResult> RegisterAsManager([FromBody] AccountRegisRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var existphone = _accountService.GetAccountByPhone(Input.Phone);
                var existemail = _accountService.GetAccountByEmail(Input.Email);
                if (existphone != null || existemail != null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Email or Phone have existed please change",
                        Data = null
                    });
                }
                var accounts = new Account()
                {
                    Phone = Input.Phone,
                    Email = Input.Email,
                    Password = Input.Password,
                    FullName = Input.Phone,                    
                    Status = false,
                    //RoleType = "Manager",
                    //ProfilePic = await Utilities.UploadFile(Input.profilePic, @"images\accounts\managers", Input.profilePic.FileName),
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.Email,

                };
                if (Input.confirmPass != Input.Password)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Sai ConfirmPass",
                        Data = null
                    });
                }
                await _accountService.Add(accounts);
                var manager = new Staff()
                {
                    AccountId = accounts.Id,                   
                    Status = false,
                    IsManager = true,
                    CenterId = null,
                    //IdFrontImg = await Utilities.UploadFile(Input.IdFrontImg, @"images\accounts\managers", Input.profilePic.FileName),
                    //IdBackImg = await Utilities.UploadFile(Input.IdBackImg, @"images\accounts\managers", Input.profilePic.FileName),
                    CreatedBy = accounts.CreatedBy,
                    CreatedDate = DateTime.Now,
                };
                await _staffService.Add(manager);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = accounts
                });
            }
            else { return BadRequest(); }

        }

        [HttpPut("{id}/verify")]
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

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            try
            {
                string id = User.FindFirst("Id")?.Value;
                var user = await _accountService.GetById(int.Parse(id));
                var token = new TokenModel();
                if (User.FindFirst(ClaimTypes.Role)?.Value == "Customer")
                {
                    token = await GenerateToken(user, false);
                }
                else if (User.FindFirst(ClaimTypes.Role)?.Value != "Customer")
                {
                    token = await GenerateToken(user, true);
                }
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = new CurrentUserResponseModel
                    {
                        //TokenId = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token).Claims.First(claim => claim.Type.ToLower().Equals("TokenId".ToLower())).Value,
                        AccountId = int.Parse(id),
                        Email = User.FindFirst(ClaimTypes.Email)?.Value,
                        Phone = User.FindFirst("Phone")?.Value,
                        RoleType = User.FindFirst(ClaimTypes.Role)?.Value,
                        LocationId = user.LocationId,
                        Name = User.FindFirst(ClaimTypes.Name)?.Value,
                        Avatar = user.ProfilePic != null ? await _cloudStorageService.GetSignedUrlAsync(user.ProfilePic) : null,
                        Gender = user.Gender,
                        Dob = user.Dob.HasValue ? (user.Dob.Value).ToString("dd-MM-yyyy") : null
                    }
                });
            } catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            
        }

        [Authorize]
        [HttpPut("profile-picture")]
        public async Task<IActionResult> UpdateProfilePic(string SavedFileName)
        {
            if (!ModelState.IsValid) {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Model is not valid",
                    Data = null
                });
            }
            else
            {
                string id = User.FindFirst("Id")?.Value;
                Customer existingCustomer = await _customerService.GetCustomerByAccID(int.Parse(id));
                if (existingCustomer == null) { 
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found customer",
                        Data = null
                    }); 
                }
                else
                {
                    var userid = existingCustomer.AccountId ?? 0;
                    Account account = await _accountService.GetById(userid);
                    account.ProfilePic = SavedFileName;

                    await _accountService.Update(account);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Updated",
                        Data = existingCustomer
                    });
                }


            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfileInfo([FromBody] CustomerRequestModel input)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                DateTime dateTime;
                bool a = DateTime.TryParseExact(input.Dob, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                if (input.Dob != null && !DateTime.TryParseExact(input.Dob, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime)) {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Datetime not exact format ",
                        Data = null
                    });
                }
                    int  id = int.Parse(User.FindFirst("Id")?.Value);
                Customer existingCustomer =  await _customerService.GetCustomerByAccID(id);
                Account user = await _accountService.GetById(id);

                if (existingCustomer == null) { 
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found customer",
                        Data = null
                    });  }
                else
                {
                    existingCustomer.Fullname = input.FullName != null ? input.FullName : existingCustomer.Fullname;
                    existingCustomer.UpdatedDate = DateTime.Now;
                    existingCustomer.UpdatedBy = existingCustomer.Email;                  
                    await _customerService.Update(existingCustomer);
                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedBy = user.FullName;
                    user.FullName = input.FullName != null ? input.FullName : user.FullName;
                    user.Gender = input.Gender != null ? input.Gender : user.Gender;
                    string format = "dd-MM-yyyy";
                    if (input.Dob != null)
                    {
                        DateTime dobInput = DateTime.ParseExact(input.Dob, format, CultureInfo.InvariantCulture);
                        int age = DateTime.Now.Year - dobInput.Year;
                        if (DateTime.Now < dobInput.AddYears(age))
                        {
                            age--;
                        }
                        if (age > 15 && age < 80)
                        {
                            user.Dob = DateTime.ParseExact(input.Dob, format, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            return BadRequest(new ResponseModel
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = "Not Updated because your age is not suitable for use this platform",
                                Data = null
                            });
                        }
                    }

                    await _accountService.Update(user);

                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Updated",
                        Data = existingCustomer
                    });
                }

            }
        }

        [Authorize]
        [HttpPut("address")]
        public async Task<IActionResult> UpdateAddressInfo([FromBody] LocationRequestModel Input)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                int accId = int.Parse(User.FindFirst("Id")?.Value);
                Customer existingCustomer = await _customerService.GetCustomerByAccID(accId);
                //var accountId = existingCustomer.AccountId;
                //int userId = accountId ?? 0;
                Account user = await _accountService.GetById(accId);

                var location = new Model.Models.Location();
                location.AddressString = Input.AddressString;
                location.WardId = Input.WardId;
                var ward = new Ward();
                try
                {
                    ward = await _wardService.GetWardById(location.WardId);
                }
                catch
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found ward by wardId.",
                        Data = null
                    });
                }
                string fullAddress = Input.AddressString + ", " + ward.WardName + ", " + ward.District.DistrictName + ", Thành phố Hồ Chí Minh";
                string url = $"https://nominatim.openstreetmap.org/search?email=thanhdat3001@gmail.com&q=={fullAddress}&format=json&limit=1";
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(json);
                        if (result.Count > 0)
                        {

                            location.Latitude = result[0].lat;
                            location.Longitude = result[0].lon;
                        }
                    }
                }
                if (Input.Latitude != null && Input.Latitude != 0)
                {
                    location.Latitude = Input.Latitude;
                }
                if (Input.Longitude != null && Input.Longitude != 0)
                {
                    location.Longitude = Input.Longitude;
                }
                if (location.Latitude != null && location.Longitude != null && location.Latitude != 0 && location.Longitude != 0)
                {
                    location.Latitude = Math.Round((decimal)location.Latitude, 9);
                    location.Longitude = Math.Round((decimal)location.Longitude, 9);
                }
                else
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Location of center(latitude and longitude) not recognized or not in Ho Chi Minh city.",
                        Data = null
                    });
                }
                var locationAdded = await _locationService.Add(location);

                if (existingCustomer == null) { return NotFound(); }
                else
                {
                    existingCustomer.Address = locationAdded.Id;
                    await _customerService.Update(existingCustomer);


                    user.LocationId = existingCustomer.Address;
                    await _accountService.Update(user);


                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Updated",
                        Data = existingCustomer
                    });
                }


            }
        }

        [HttpGet("my-wallet")]
        public async Task<IActionResult> GetMyWallet()
        {
            int id = int.Parse(User.FindFirst("Id")?.Value);
            var account = await _accountService.GetById(id);
            if (account == null)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Account not found",
                    Data = null
                });
            } else
            {
                if (account.Wallet == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Account do not have wallet",
                        Data = null
                    });
                } else
                {
                    var transactions = new List<TransactionResponseModel>();
                    foreach (var item in account.Wallet.Transactions)
                    {
                        string _plusOrMinus = "minus";
                        if (item.Type.ToLower().Equals("deposit"))
                        {
                            _plusOrMinus = "plus";
                        }
                        transactions.Add(new TransactionResponseModel
                        {
                            Type = item.Type,
                            Status = item.Status,
                            PlusOrMinus = _plusOrMinus,
                            Amount = item.Amount,
                            TimeStamp = item.TimeStamp.ToString("dd-MM-yyyy HH:mm:ss")
                        });
                    }
                    var walletTransactions = new List<WalletTransactionResponseModel>();
                    foreach (var item in account.Wallet.WalletTransactionFromWallets)
                    {
                        walletTransactions.Add(new WalletTransactionResponseModel
                        {
                            PaymentId = item.PaymentId,
                            Type = item.Type,
                            Status = item.Status,
                            PlusOrMinus = "Minus",
                            Amount = item.Amount,
                            TimeStamp = item.TimeStamp.ToString("dd-MM-yyyy HH:mm:ss"),
                            UpdateTimeStamp = item.UpdateTimeStamp.HasValue ? item.UpdateTimeStamp.Value.ToString("dd-MM-yyyy HH:mm:ss") : null
                        });
                    }
                    foreach (var item in account.Wallet.WalletTransactionToWallets)
                    {
                        walletTransactions.Add(new WalletTransactionResponseModel
                        {
                            PaymentId = item.PaymentId,
                            Type = item.Type,
                            Status = item.Status,
                            PlusOrMinus = "Plus",
                            Amount = item.Amount,
                            TimeStamp = item.TimeStamp.ToString("dd-MM-yyyy HH:mm:ss"),
                            UpdateTimeStamp = item.UpdateTimeStamp.HasValue ? item.UpdateTimeStamp.Value.ToString("dd-MM-yyyy HH:mm:ss") : null
                        });
                    }
                    return Ok(new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = new WalletResponseModel
                        {
                            WalletId = account.Wallet.Id,
                            Balance = account.Wallet.Balance,
                            Status = account.Wallet.Status,
                            Transactions = transactions.OrderByDescending(tran => DateTime.ParseExact(tran.TimeStamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList(),
                            WalletTransactions = walletTransactions.OrderByDescending(tran => DateTime.ParseExact(tran.TimeStamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList()
                        }
                    });
                }
            }

        }

    }
}
