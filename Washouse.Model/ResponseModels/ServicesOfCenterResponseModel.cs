using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class ServicesOfCenterResponseModel
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int? TimeEstimate { get; set; }
        public decimal Rating { get; set; }
        public int NumOfRating { get; set; }

    }
}
