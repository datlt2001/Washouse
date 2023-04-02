using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class SendSMSRequestModel
    {
        public string MobileNumber { get; set; }
        public string Body { get; set; }
    }
}
