using System;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderDeliveryModel
    {
        public string ShipperName { get; set; }
        public string ShipperPhone { get; set; }
        public int LocationId { get; set; }
        public string AddressString { get; set; }
        public bool DeliveryType { get; set; }
        public int? EstimatedTime { get; set; }
        public string DeliveryDate { get; set; }
        public string Status { get; set; }
    }
}