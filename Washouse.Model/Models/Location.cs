using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Washouse.Model.Models
{
    [Table("Locations")]
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string AddressString { get; set; }
        [Required]
        public int WardId { get; set; }
        [ForeignKey("WardId")]
        public virtual Ward Ward { get; set; }
        public virtual IEnumerable<Center> Centers { get; set; }
        public virtual IEnumerable<Delivery> Deliveries { get; set; }

    }
}
