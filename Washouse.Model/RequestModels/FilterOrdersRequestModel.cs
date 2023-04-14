using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class FilterOrdersRequestModel
    {
        public FilterOrdersRequestModel()
        {
            Page = 1;
            PageSize = 10;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? SearchString { get; set; }
        public int? DeliveryType { get; set; }
        public string? DeliveryStatus { get; set; }
        public string FromDate { get; set; }    
        public string ToDate { get; set; }    
        public string Status { get; set; }    
    }
}
