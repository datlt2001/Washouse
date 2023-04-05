using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class StaffCenterModel
    {
        public bool Status { get; set; }
        public string IdNumber { get; set; }
        public string IdFrontImg { get; set; }
        public string IdBackImg { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string ProfilePic { get; set; }
        public string StaffAddress { get; set; }
    }
}
