using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class AccountGoogleRegisRequestModel
    {
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string? SavedFileName { get; set; }
    }
}
