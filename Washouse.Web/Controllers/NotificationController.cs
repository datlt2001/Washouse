using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        public INotificationService _notificationService;
        public IAccountService _accountService;
        public INotificationAccountService _notificationAccountService;

        public NotificationController(INotificationService notificationService, IAccountService accountService, INotificationAccountService notificationAccountService)
        {
            _notificationService = notificationService;
            _accountService = accountService;
            _notificationAccountService = notificationAccountService;
        }
        //[HttpGet("unread")]
        //public IActionResult GetUnreadNotifications(int accId)
        //{
        //    string id = User.FindFirst("Id")?.Value;
        //    //int accId = int.Parse(id);
        //    var notis = _notificationService.GetNotificationUnread(accId);

        //    return Ok(new ResponseModel
        //    {
        //        StatusCode = 0,
        //        Message = "success",
        //        Data = notis
        //    });
        //}

        //[HttpGet("read")]
        //public IActionResult GetReadNotifications(int accId)
        //{
        //    string id = User.FindFirst("Id")?.Value;
        //    //int accId = int.Parse(id);
        //    var notis = _notificationService.GetNotificationRead(accId);
        //    return Ok(new ResponseModel
        //    {
        //        StatusCode = 0,
        //        Message = "success",
        //        Data = notis
        //    });
        //}

        [HttpGet("account/{accountId}")]
        public IActionResult GetNotifications(int accountId, bool isRead)
        {
            IEnumerable<NotificationViewModel> notis = null ;
            if(isRead == true)
            {
                 notis = _notificationService.GetNotificationRead(accountId);
            }
            else
            {
                notis = _notificationService.GetNotificationUnread(accountId);
            }
            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "success",
                Data = notis
            });
        }

        [HttpPost("read")]
        public async Task<IActionResult> UpdateNotification(int notiId)
        {

            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                NotificationAccount noti =  _notificationAccountService.GetNotiAccbyNotiId(notiId);
                if (noti == null) { return NotFound(); }
                noti.ReadDate = DateTime.Now;
                await _notificationAccountService.Update(noti);
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = noti
                });
            }
        }
    }
}
