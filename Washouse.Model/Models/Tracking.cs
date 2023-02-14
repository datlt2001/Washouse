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
    [Table("Trackings")]
    public class Tracking : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public bool Status { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
