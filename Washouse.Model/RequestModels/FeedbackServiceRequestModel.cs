using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class FeedbackServiceRequestModel
    {
        public int ServiceId { get; set; }
        public int CenterId { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
    }
}
