
using Washouse.Model.ViewModel;

namespace Washouse.Model.RequestModels
{
    public class FilterServicesOfCenterRequestModel : PaginationViewModel
    {
        public string SearchString { get; set; }
    }
}
