using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.ViewModel;

namespace Washouse.Data.Repositories
{
    public interface INotificationRepository :IRepository<Notification>
    {
        public IEnumerable<NotificationViewModel> GetNotificationUnread(int accountId);
        public IEnumerable<NotificationViewModel> GetNotificationRead(int accountId);
        public int CountNotificationUnread(int accountId);
         IEnumerable<NotificationViewModel> GetNotifications(int accountId);
    }
}
