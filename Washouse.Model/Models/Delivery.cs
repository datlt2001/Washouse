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
    [Table("Deliveries")]
    public class Delivery : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        public string ShipperName { get; set; }
        public string ShipperPhone { get; set; }
        [Required]
        public int LocationId { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public DateTime DeliveryDate { get; set; }
        [Required]
        public bool Status { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
