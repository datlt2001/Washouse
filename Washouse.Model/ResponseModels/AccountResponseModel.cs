using System;

namespace Washouse.Model.ResponseModels
{
    public class AccountResponseModel
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public bool Status { get; set; }
        public bool IsAdmin { get; set; }
        public string ProfilePic { get; set; }
    }
}
