using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using Washouse.Service.Interface;
using Washouse.Model.RequestModels;
using System;
using Twilio.Rest.Verify.V2.Service;
using Twilio;
using Microsoft.AspNetCore.Http;
using Washouse.Web.Models;
using static System.Net.WebRequestMethods;
using Washouse.Model.ResponseModels;
using System.ComponentModel.DataAnnotations;
using Washouse.Model.Models;
using Washouse.Service.Implement;
using Washouse.Common.Mails;

namespace Washouse.Web.Controllers
{
    [Route("api/logins")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ISMSService _smsService;
        private IAccountService _accountService;
        //private ISendMailService _sendMailService;
        private ICustomerService _customerService;
        public IStaffService _staffService;

        public LoginController(ISMSService smsService, IAccountService accountService, ICustomerService customerService,
                                IStaffService staffService )
        {
            _smsService = smsService;
            _accountService = accountService;
            _customerService = customerService;
            _staffService = staffService;
        }

        //[Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [HttpGet("google")]
        public IActionResult LoginWithGoogle()
        {
            string returnUrl = "/api/logins/google-callback";

            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        
        //[Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {

            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                // handle authentication failure
                return BadRequest();
            }
            var Token = await HttpContext.GetTokenAsync(GoogleDefaults.AuthenticationScheme, "access_token");
            var externalClaims = result.Principal.Claims.ToList();
            var subjectIdClaim = externalClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var subjectNameClaim = externalClaims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

            //string phoneNumber = subjectPhoneClaim?.Value;

            var accessToken = result.Properties.GetTokenValue("id_token");
            var client = new HttpClient();
            GoogleAccountResponseModel responseModel = new GoogleAccountResponseModel();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);          
                var response = await client.GetAsync("https://people.googleapis.com/v1/people/me?personFields=emailAddresses,names,phoneNumbers,photos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);
                    var phoneNumber = json["phoneNumbers"]?.FirstOrDefault()?["value"]?.ToString();
                    var imageUrl = json["photos"]?.FirstOrDefault()?["url"]?.ToString();
                    var email = json["emailAddresses"]?.FirstOrDefault()?["value"]?.ToString();
                    
                    responseModel.Fullname = subjectNameClaim.Value;
                    responseModel.Email = email;
                    responseModel.Image= imageUrl;
                }
                


            // create claims for the new user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, subjectIdClaim.Value),
                new Claim(ClaimTypes.Name, subjectNameClaim.Value),
                //new Claim(ClaimTypes.MobilePhone, subjectPhoneClaim.Value),
                // add additional claims here
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);
            //// Do something with the user's information
            //var email = result.Principal.FindFirst(ClaimTypes.Email).Value;

            //// Redirect the user to the original URL
            

            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = responseModel
            });
        }

        

        [HttpPost("customers")]
        public async Task<IActionResult> RegisterAsCustomer([FromBody] AccountGoogleRegisRequestModel Input)
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
                    //Password = Input.Password,
                    Status = false,
                    FullName = Input.Fullname,                  
                    ProfilePic = Input.SavedFileName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = Input.Email,

                };             
                await _accountService.Add(accounts);
                var customer = new Customer()
                {
                    AccountId = accounts.Id,
                    Fullname = accounts.FullName,
                    Phone = accounts.Phone,
                    Email = accounts.Email,
                    Status = false,
                    CreatedBy = accounts.CreatedBy,
                    CreatedDate = DateTime.Now,
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
    }
}
