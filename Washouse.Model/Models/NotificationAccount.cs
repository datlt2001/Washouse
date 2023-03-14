using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class NotificationAccount
    {
        public int NotificationId { get; set; }
        public int AccountId { get; set; }
        public DateTime? ReadDate { get; set; }

        public virtual Account Account { get; set; }
        public virtual Notification Notification { get; set; }
    }
}
