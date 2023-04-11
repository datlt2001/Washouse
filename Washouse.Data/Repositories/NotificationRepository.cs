using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.ViewModel;

namespace Washouse.Data.Repositories
{
    public class NotificationRepository :RepositoryBase<Notification>, INotificationRepository
    {
        public NotificationRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public IEnumerable<NotificationViewModel> GetNotificationUnread(int accountId)
        {
            var notifications = from n in DbContext.Notifications
                                join na in DbContext.NotificationAccounts on n.Id equals na.NotificationId
                                where na.AccountId == accountId && na.ReadDate == null
                                select new NotificationViewModel
                                {
                                    Id = n.Id,
                                    Title = n.Title,
                                    Content = n.Content,
                                    CreatedDate = n.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                                    OrderId = n.OrderId,
                                    AccountId = na.AccountId,
                                    IsRead = false
                                };

            return notifications.ToList();
        }

        public IEnumerable<NotificationViewModel> GetNotificationRead(int accountId)
        {
            var notifications = from n in DbContext.Notifications
                                join na in DbContext.NotificationAccounts on n.Id equals na.NotificationId
                                where na.AccountId == accountId && na.ReadDate != null
                                select new NotificationViewModel
                                {
                                    Id = n.Id,
                                    Title = n.Title,
                                    Content = n.Content,
                                    CreatedDate = n.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                                    OrderId = n.OrderId,
                                    AccountId = na.AccountId,
                                    IsRead = true
                                    
                                };

            return notifications.ToList();
        }

        public int CountNotificationUnread(int accountId)
        {
            var count = (from n in DbContext.Notifications
                         join na in DbContext.NotificationAccounts on n.Id equals na.NotificationId
                         where na.AccountId == accountId && na.ReadDate == null
                         select n).Count();

            return count;
        }

        public IEnumerable<NotificationViewModel> GetNotifications(int accountId)
        {
            var notifications = from n in DbContext.Notifications
                                join na in DbContext.NotificationAccounts on n.Id equals na.NotificationId
                                where na.AccountId == accountId 
                                select new NotificationViewModel
                                {
                                    Id = n.Id,
                                    Title = n.Title,
                                    Content = n.Content,
                                    CreatedDate = n.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                                    OrderId = n.OrderId,
                                    AccountId = na.AccountId,
                                    IsRead = na.ReadDate != null ? true : false
                                };

            return notifications.ToList();
        }
    }
}
