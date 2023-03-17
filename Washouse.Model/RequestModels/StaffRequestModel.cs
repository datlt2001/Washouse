using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class StaffRequestModel
    {
        [Required]
        public string Phone { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        //public IFormFile profilePic { get; set; }
        public string IdNumber { get; set; }
        //public IFormFile IdFrontImg { get; set; }
        //public IFormFile IdBackImg { get; set; }
        public int? LocationId { get; set; }        

    }
}
