using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Washouse.Model.Models
{
    [Table("ServicePrices")]
    public class ServicePrice
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ServiceId { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal Price { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
}
