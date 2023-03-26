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


    }
}
