using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CenterEditRequestModel : CenterRequestModel
    {
        public LocationRequestModel? Location { get; set; }
    }
}
