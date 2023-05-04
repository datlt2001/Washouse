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

namespace Washouse.Web.Controllers
{
    [Route("api/verifys")]
    [ApiController]
    public class VerifyController : ControllerBase
    {
        private readonly ISMSService _smsService;
        private readonly IDistributedCache _cache;
        private ISendMailService _sendMailService;

        public VerifyController(ISMSService smsService, IDistributedCache cache, ISendMailService sendMailService)
        {
            _smsService = smsService;
            _cache = cache;
            _sendMailService = sendMailService;
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
            //RedisCache.Set("OTP", otp, TimeSpan.FromMinutes(5));
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(3));
            string jsonString = JsonSerializer.Serialize(otp);
            byte[] objectDataAsStream = Encoding.UTF8.GetBytes(jsonString);
            _cache.Set("OTP", objectDataAsStream, options);

            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Send Successfully",
                Data = null
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
            _cache.Set("OTPMail", objectDataAsStream, options);
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Send Successfully",
                Data = null
            });
        }

        [HttpPost("sms/{otp}")]
        public IActionResult CheckOTP(string otp)
        {
            var res = _cache.Get("OTP");
            string result = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(res));
            if (otp.Equals(result))
            {
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Verify success",
                    Data = result
                });
            }
            else
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Wrong OTP",
                    Data = result
                });
            }



        }

        [HttpPost("mail/{otp}")]
        public IActionResult CheckOTPMail(string otp)
        {
            var res = _cache.Get("OTPMail");
            string result = JsonSerializer.Deserialize<string>(Encoding.UTF8.GetString(res));
            if (otp.Equals(result))
            {
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Verify success",
                    Data = result
                });
            }
            else
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Wrong OTP",
                    Data = result
                });
            }
        }
    }
}
