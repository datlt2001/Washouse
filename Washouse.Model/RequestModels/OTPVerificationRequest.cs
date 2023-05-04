using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class OTPVerificationRequest
    {
        public string otp { get; set; }
        public string phonenumber { get; set; }
    }
}
