using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;
using Washouse.Service.Interface;
using Washouse.Web.Hub;
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
        private IHubContext<MessageHub, IMessageHubClient> messageHub;

        public NotificationController(INotificationService notificationService, IAccountService accountService, 
            INotificationAccountService notificationAccountService, IHubContext<MessageHub, IMessageHubClient> _messageHub)
        {
            _notificationService = notificationService;
            _accountService = accountService;
            _notificationAccountService = notificationAccountService;
            messageHub = _messageHub;
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

        [Authorize]
        [HttpGet("me-noti")]
        public IActionResult GetNotifications(bool? isRead)
        {
            int accountId = int.Parse(User.FindFirst("Id").Value);
            IEnumerable<NotificationViewModel> notis = null;                   
            if (isRead == null) 
            {

                notis = _notificationService.GetNotifications(accountId);
            }
            else if (isRead != null && isRead == true)
            {
                notis = _notificationService.GetNotificationRead(accountId);

            }
            else if (isRead != null && isRead == false)
            {
                notis = _notificationService.GetNotificationUnread(accountId);

            }
            return Ok(new ResponseModel
            {
                StatusCode = 0,
                Message = "success",
                Data = new
                {
                    NumOfUnread = _notificationService.CountNotificationUnread(accountId),
                    Notifications =notis
                }
            });
        }

        [Authorize]
        [HttpPost("read")]
        public async Task<IActionResult> UpdateNotification(int notiId)
        {

            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                NotificationAccount noti = _notificationAccountService.GetNotiAccbyNotiId(notiId);
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

        [HttpPost]
        [Route("notificationsoffers")]
        public string Get()
        {
            messageHub.Clients.All.NotifyToUser("Notification");
            return "Offers sent successfully to all users!";
        }
    }
}
