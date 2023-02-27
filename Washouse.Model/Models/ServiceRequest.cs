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
    [Table("ServiceRequests")]
    public class ServiceRequest : Auditable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServiceRequesting { get; set; }
        [Required]
        public bool RequestStatus { get; set; }

        [Required]
        [MaxLength(256)]
        public string ServiceName { get; set; }
        [Required]
        [MaxLength(256)]
        public string Alias { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public bool PriceType { get; set; }
        [MaxLength(256)]
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public int TimeEstimate { get; set; }
        public bool Status { get; set; }
        public bool? HomeFlag { get; set; }
        public bool? HotFlag { get; set; }
        public decimal Rating { get; set; }

        [ForeignKey("ServiceRequesting")]
        public virtual Service Service { get; set; }
    }
}
