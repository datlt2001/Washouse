using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CenterOfOrderModel
    {
        public int CenterId { get; set; }
        public string CenterName { get; set; }
        public string CenterAddress { get; set; }
        public string CenterPhone { get; set; }
    }
}
