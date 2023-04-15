using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ViewModel;

namespace Washouse.Model.RequestModels
{
    public class FilterOrdersRequestModel : PaginationViewModel
    {
        public string? SearchString { get; set; }
        public bool? DeliveryType { get; set; }
        public string? DeliveryStatus { get; set; }
        public string FromDate { get; set; }    
        public string ToDate { get; set; }    
        public string Status { get; set; }    
    }
}
