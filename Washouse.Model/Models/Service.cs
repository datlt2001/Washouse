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
    [Table("Services")]
    public class Service : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
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
        public string Status { get; set; }
        public bool? HomeFlag { get; set; }
        public bool? HotFlag { get; set; }
        public decimal Rating { get; set; }
        [Required]
        public int CenterId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        [ForeignKey("CenterId")]
        public virtual Center Center { get; set; }

        public virtual IEnumerable<ServicePrice> ServicePrices { get; set; }
        public virtual ICollection<ServiceGallery> ServiceGalleries { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
    }
}
