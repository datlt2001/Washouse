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
    [Table("Customers")]
    public class Customer : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public bool Status { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
