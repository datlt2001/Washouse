using System;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class PromotionCenterModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public int? UseTimes { get; set; }
        public bool IsAvailable { get; set; }
    }
}