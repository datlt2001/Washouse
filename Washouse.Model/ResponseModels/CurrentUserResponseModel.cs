using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CurrentUserResponseModel
    {
        public string TokenId { get; set; }
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleType { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int? LocationId { get; set; }
        public int? Gender { get; set; }
        public string Dob { get; set; }
    }
}
