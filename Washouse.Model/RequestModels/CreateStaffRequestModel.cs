using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CreateStaffRequestModel
    {
        public string Email { get; set; }

        public string FullName { get; set; }
        
        public string IdNumber { get; set; }
        public string Phone { get; set; }

        public string Password { get; set; }
    }
}
