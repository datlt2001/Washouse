using System;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderPaymentModel
    {
        public decimal PaymentTotal { get; set; }
        public decimal PlatformFee { get; set; }
        public string DateIssue { get; set; }
        public string Status { get; set; }
        public int PaymentMethod { get; set; }
        public string PromoCode { get; set; }
        public decimal? Discount { get; set; }
        public string CreatedDate { get; set; }
        //public string CreatedBy { get; set; }
        public string UpdatedDate { get; set; }
        //public string UpdatedBy { get; set; }
    }
}