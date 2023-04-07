using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;

namespace Washouse.Model.RequestModels
{
    public class FilterCentersRequestModel
    {
        public FilterCentersRequestModel()
        {
            Page = 1;
            PageSize = 10;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Sort { get; set; }
        public string? BudgetRange { get; set; }
        public string? CategoryServices { get; set; }
        public string? Additions { get; set; }
        public string? SearchString { get; set; }
        public bool HasDelivery { get; set; }
        public bool HasOnlinePayment { get; set; }
        public decimal? CurrentUserLatitude { get; set; }
        public decimal? CurrentUserLongitude { get; set; }
    }
}
