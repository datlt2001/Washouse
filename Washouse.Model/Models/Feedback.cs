using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Feedback
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string OrderId { get; set; }
        public int? CenterId { get; set; }
        public int? ServiceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ReplyMessage { get; set; }
        public string ReplyBy { get; set; }
        public DateTime? ReplyDate { get; set; }

        public virtual Center Center { get; set; }
        public virtual Order Order { get; set; }
        public virtual Service Service { get; set; }
    }
}
