using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CustomerRequestModel
    {
        [Required]
        public string Phone { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string confirmPass { get; set; }
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        //public IFormFile profilePic { get; set; }
        public int? LocationId { get; set; }

    }
}
