using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Washouse.Model.Models
{
    [Table("Districts")]
    public class District
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string DistrictName { get; set; }

        public virtual IEnumerable<Ward> Wards { get; set; }
    }
}
