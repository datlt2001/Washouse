using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ViewModel;

namespace Washouse.Model.RequestModels
{
    public class FilterCustomersOfCenterRequestModel
    {
        public PaginationViewModel Pagination { get; set; }
        public string SearchString { get; set; }
    }
}
