using System;

namespace Washouse.Model.RequestModels
{
    public class DeliveryRequestModel
    {
        public string AddressString { get; set; }
        public int WardId { get; set; }
        public  bool DeliveryType { get; set;}
        //public DateTime   { get; set; }
    }
}