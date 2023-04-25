using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderInfomationModel
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public int LocationId { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerMessage { get; set; }
        public int CustomerOrdered { get; set; }
        public decimal TotalOrderValue { get; set; }
        public int DeliveryType { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public string PreferredDropoffTime { get; set; }
        public string PreferredDeliverTime { get; set; }
        public string CancelReasonByStaff { get; set; }
        public string CancelReasonByCustomer { get; set; }
        public string Status { get; set; }
        public List<OrderDetailInfomationModel> OrderedDetails { get; set; }
        public List<OrderTrackingModel> OrderTrackings { get; set; }
        public List<OrderDeliveryModel> OrderDeliveries { get; set; }
        public OrderPaymentModel OrderPayment { get; set; }
        public CenterOfOrderModel Center { get; set; }
        public FeedbackCenterModel? Feedback { get; set; }
    }
}
