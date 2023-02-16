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
    [Table("CenterGalleries")]
    public class CenterGallery
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CenterId { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [ForeignKey("CenterId")]
        public virtual Center Center { get; set; }
    }
}
