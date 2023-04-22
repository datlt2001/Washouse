using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Washouse.Web.Models
{
    public class LoginGoogleModel
    {
        [Required] public string Code { get; set; }
        [Required] public string RedirectUri { get; set; }
    }
}