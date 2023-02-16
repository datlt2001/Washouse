using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Washouse.Model.Abstract;

namespace Washouse.Model.Models
{
    [Table("Accounts")]
    public class Account : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Phone { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public string RoleType { get; set; }
        public string ProfilePic { get; set; }
        [Required]
        public bool IsResetPassword { get; set; }
        //public virtual IEnumerable<Post> Posts { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
