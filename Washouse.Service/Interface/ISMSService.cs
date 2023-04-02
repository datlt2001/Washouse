using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace Washouse.Service.Interface
{
    public interface ISMSService
    {
        MessageResource Send(string mobileNumber, string body);
    }
}
