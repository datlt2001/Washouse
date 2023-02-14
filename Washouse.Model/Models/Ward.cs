using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Washouse.Model.Models
{
    [Table("Wards")]
    public class Ward
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string WardName { get; set; }
        [Required]
        public int DistrictId { get; set; }
        [ForeignKey("DistrictId")]
        public virtual District District { get; set; }
        public virtual IEnumerable<Location> Locations { get; set; }
    }
}
