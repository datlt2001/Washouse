using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class PromotionRequestModel
    {
        [Required]
        public string Code { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal Discount { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public int? UseTimes { get; set; }

    }
}
