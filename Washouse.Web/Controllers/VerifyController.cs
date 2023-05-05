using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using System.Text.Json;
using Washouse.Common.Mails;
using Washouse.Model.Models;
using System.Threading.Tasks;
using Washouse.Model.RequestModels;

namespace Washouse.Web.Controllers
{
    [Route("api/verifys")]
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly ISMSService _smsService;
        private readonly IDistributedCache _cache;
        private ISendMailService _sendMailService;
        private IAccountService _accountService;

        public VerifyController(ISMSService smsService, IDistributedCache cache, ISendMailService sendMailService, IAccountService accountService)
        {
            _smsService = smsService;
            _cache = cache;
            _sendMailService = sendMailService;
            _accountService = accountService;
        }

        [HttpPost("send/otp")]
        public IActionResult SendOTP(string phoneNumber)
        {
            Random random = new Random();
            string otp = random.Next(1000, 9999).ToString();
            string sdt = "0975926021";

            string formattedPhoneNumber = "+84" + sdt.Substring(1);
            var result = _smsService.Send(formattedPhoneNumber, otp);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
                return BadRequest(result.ErrorMessage);           
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));         
            _cache.SetString(phoneNumber, otp, options);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Send Successfully",
                Data = otp
            });
        }

        [HttpPost("send/otp-login")]
        public  IActionResult SendOTPLogin(string phoneNumber)
        {
            var user = _accountService.GetAccountByPhone(phoneNumber);
            if (user == null)
            {
                return Ok(new ResponseModel
                {
                    StatusCode = 10,
                    Message = "Invalid phone",
                    Data = null
                });
            }
            if (user.IsAdmin)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Admin can not login with OTP",
                    Data = null
                });
            }
            Random random = new Random();
            string otp = random.Next(1000, 9999).ToString();
            string sdt = "0975926021";

            string formattedPhoneNumber = "+84" + sdt.Substring(1);
            var result = _smsService.Send(formattedPhoneNumber, otp);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
                return BadRequest(result.ErrorMessage);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cache.SetString(phoneNumber, otp, options);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Send Successfully",
                Data = otp
            });
        }

        [HttpPost("send/mail")]
        public async Task<IActionResult> SendMail(string email)
        {
            string path = "./Templates_email/ForgotPassword.txt";
            string content = System.IO.File.ReadAllText(path);
            Random random = new Random();
            string otp = random.Next(1000, 9999).ToString();
            content = content.Replace("{recipient}", email);
            content = content.Replace("{otp}", otp);          
            await _sendMailService.SendEmailAsync(email, "Mã OTP Washouse", content);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(3));
            string jsonString = JsonSerializer.Serialize(otp);
            byte[] objectDataAsStream = Encoding.UTF8.GetBytes(jsonString);          
            _cache.SetString(email, otp, options);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Send Successfully",
                Data = otp
            });
        }

        [HttpPost("sms/check")]
        public IActionResult CheckOTP([FromBody] OTPVerificationRequest request)
        {
            var result = _cache.GetString(request.phonenumber);
            //string result = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(res));
            if(result == null) 
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "fail",
                    Data = null
                });
            }
            if (request.otp.Equals(result))
            {
                _cache.Remove(request.phonenumber);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = null
                });
            }
            else
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "fail",
                    Data = null
                });
            }



        }

        [HttpPost("mail/check")]
        public IActionResult CheckOTPMail([FromBody] MailVerificationRequest request)
        {
            var result = _cache.GetString(request.email);
            //string result = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(res));
            if (result == null)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "fail",
                    Data = null
                });
            }
            if (request.otp.Equals(result))
            {
                _cache.Remove(request.email);
                return Ok(new ResponseModel
                {

                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = null
                });
            }
            else
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "fail",
                    Data = null
                });
            }
        }
    }
}
