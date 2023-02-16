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
    [Table("Staffs")]
    public class Staff : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public bool IsManager { get; set; }
        [Required]
        public int CenterId { get; set; }
        public string IdNumber { get; set; }
        public string IdFrontImg { get; set; }
        public string IdBackImg { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        [ForeignKey("CenterId")]
        public virtual Center Center { get; set; }
    }
}
