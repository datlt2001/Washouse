using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CenterOperatingHoursResponseModel
    {
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

    }
}
