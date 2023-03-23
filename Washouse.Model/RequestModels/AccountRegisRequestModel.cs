using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class AccountRegisRequestModel
    {
        public string Phone { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string confirmPass { get; set; }
    }
}
