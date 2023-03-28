using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class VNPaySettings
    {
        public string VNP_TmnCode { get; set; }
        public string VNP_HashSecret { get; set; }
        public string VNP_Url { get; set; }
        public string VNP_ReturnUrl { get; set; }
    }
}
