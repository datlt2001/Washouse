using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class FeedbackResponseModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string OrderId { get; set; }
        public int? CenterId { get; set; }
        public string CenterName { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ReplyMessage { get; set; }
        public string ReplyBy { get; set; }
        public string ReplyDate { get; set; }
    }
}
