using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class PromotionResponseModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public int? UseTimes { get; set; }
        public bool IsAvailable { get; set; }
    }
}
