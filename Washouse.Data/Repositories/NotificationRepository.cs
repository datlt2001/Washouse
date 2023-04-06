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
                                    Content = n.Content,
                                    CreatedDate = n.CreatedDate.ToString("dd-MM-yyyy HH-mm-ss"),
                                    OrderId = n.OrderId,
                                    AccountId = na.AccountId
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
                                    Content = n.Content,
                                    CreatedDate = n.CreatedDate.ToString("dd-MM-yyyy HH-mm-ss"),
                                    OrderId = n.OrderId,
                                    AccountId = na.AccountId
                                };

            return notifications.ToList();
        }
    }
}
