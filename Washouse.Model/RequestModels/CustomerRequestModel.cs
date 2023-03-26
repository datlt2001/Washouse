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
        
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        //public IFormFile profilePic { get; set; }
        public string? SavedFileName { get; set; }
        //public LocationRequestModel Location { get; set; }

    }
}
