using Microsoft.AspNetCore.Mvc;
using Washouse.Service.Interface;

namespace Washouse.Web.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        public INotificationService _notificationService;
        public IAccountService _accountService;

        public NotificationController(INotificationService notificationService, IAccountService accountService)
        {
            _notificationService = notificationService;
            _accountService = accountService;
        }
        [HttpGet("unread")]
        public IActionResult GetUnreadNotifications(int accId)
        {
            string id = User.FindFirst("Id")?.Value;
            //int accId = int.Parse(id);
            var notis = _notificationService.GetNotificationUnread(accId);

            return Ok(notis);
        }

        [HttpGet("read")]
        public IActionResult GetReadNotifications(int accId)
        {
            string id = User.FindFirst("Id")?.Value;
            //int accId = int.Parse(id);
            var notis = _notificationService.GetNotificationRead(accId);

            return Ok(notis);
        }

    }
}
