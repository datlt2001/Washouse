using Washouse.Model.ViewModel;

namespace Washouse.Model.RequestModels
{
    public class GetCenterFeedbacksModel : PaginationViewModel
    {
        public int? ServiceId { get; set; }
        public string? OrderId { get; set; }
    }
}