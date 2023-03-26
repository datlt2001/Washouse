using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Account : Auditable
    {
        public Account()
        {
            Customers = new HashSet<Customer>();
            NotificationAccounts = new HashSet<NotificationAccount>();
            Posts = new HashSet<Post>();
            staff = new HashSet<Staff>();
            RefreshTokens = new HashSet<RefreshToken>();
        }

        public int Id { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        public bool Status { get; set; }
        public bool IsAdmin { get; set; }
        public string ProfilePic { get; set; }
        public int? LocationId { get; set; }
        public bool IsResetPassword { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual Location Location { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<NotificationAccount> NotificationAccounts { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Staff> staff { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
