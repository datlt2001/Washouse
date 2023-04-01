using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class WardResponseModel
    {
        public int WardId { get; set; }
        public string WardName { get; set; }
        public DistrictResponseModel District { get; set; }
        
    }
}
