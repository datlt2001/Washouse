using System;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class FeedbackCenterModel
    {
        public string Content { get; set; }
        public int Rating { get; set; }
        //public string OrderId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ReplyMessage { get; set; }
        public string ReplyBy { get; set; }
        public DateTime? ReplyDate { get; set; }
    }
}