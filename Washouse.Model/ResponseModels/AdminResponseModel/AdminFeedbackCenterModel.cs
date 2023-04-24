using System.Collections.Generic;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class AdminFeedbackCenterModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ReplyMessage { get; set; }
        public string ReplyBy { get; set; }
        public string ReplyDate { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<AdminFeedbackServiceModel> Services { get; set; }
    }
}