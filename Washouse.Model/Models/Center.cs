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
    [Table("Centers")]
    public class Center : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CenterName { get; set; }
        public string Alias { get; set; }
        [Required]
        public int LocationId { get; set; }
        [Required]
        public string Description { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int? MonthOff { get; set; }
        public string WeekOff { get; set; }
        [Required]
        public bool Status { get; set; }
        public string Image { get; set; }
        public bool? HotFlag { get; set; }
        public decimal Rating { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        public virtual IEnumerable<Service> Services { get; set; }
    }
}
