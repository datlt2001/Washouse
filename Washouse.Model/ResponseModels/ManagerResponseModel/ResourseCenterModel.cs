using System;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class ResourseCenterModel
    {
        public string ResourceName { get; set; }
        public string Alias { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal? WashCapacity { get; set; }
        public decimal? DryCapacity { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}