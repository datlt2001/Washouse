using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CustomerLocatonResponseModel
    {
        public DistrictResponseModel District { get; set; }
        public WardResponseModel Ward { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
