using System;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class PromotionCenterModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UseTimes { get; set; }
    }
}