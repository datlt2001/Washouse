using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class UpdatePromotionRequestModel
    {
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public int? UseTimes { get; set; }
    }
}
