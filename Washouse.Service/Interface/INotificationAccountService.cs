using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface INotificationAccountService
    {
        public Task Add(NotificationAccount notificationAccount);

        public Task Update(NotificationAccount notificationAccount);

        IEnumerable<NotificationAccount> GetAll();

        public Task<NotificationAccount> GetById(int id);

        public NotificationAccount GetNotiAccbyNotiId(int notiId, int accountId);
    }
}
