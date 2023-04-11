using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Notification
    {
        public Notification()
        {
            NotificationAccounts = new HashSet<NotificationAccount>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrderId { get; set; }

        public virtual Order Order { get; set; }
        public virtual ICollection<NotificationAccount> NotificationAccounts { get; set; }
    }
}
