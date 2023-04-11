using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ViewModel
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string OrderId { get; set; }
        public int AccountId { get; set; }
        public string CreatedDate { get; set; }
        public bool IsRead { get; set; }

    }
}
