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
    [Table("CenterRequests")]
    public class CenterRequest : Auditable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CenterRequesting { get; set; }
        [Required]
        public bool RequestStatus { get; set; }

        [Required]
        public string CenterName { get; set; }
        public string Alias { get; set; }
        public int LocationId { get; set; }
        public string Description { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int? MonthOff { get; set; }
        public string WeekOff { get; set; }
        public bool Status { get; set; }
        public string Image { get; set; }
        public bool? HotFlag { get; set; }
        public decimal Rating { get; set; }

        [ForeignKey("CenterRequestId")]
        public virtual Center Center { get; set; }
    }
}
