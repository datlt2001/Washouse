using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class NotificationAccountRepository : RepositoryBase<NotificationAccount>, INotificationAccountRepository
    {
        public NotificationAccountRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public NotificationAccount GetNotiAccbyNotiId(int notiId, int accountId)
        {
            return this.DbContext.NotificationAccounts.FirstOrDefault(n =>
                n.NotificationId == notiId && n.AccountId == accountId);
        }
    }
}