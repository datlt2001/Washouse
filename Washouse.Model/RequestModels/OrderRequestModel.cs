namespace Washouse.Model.RequestModels
{
    public class OrderRequestModel
    {
        public string CustomerName { get; set; }
        public string CustomerAddressString { get; set; }
        public int CustomerWardId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerMessage { get; set; }
        public int CustomerId { get; set; }
        public bool? DeliveryChoosen { get; set; }
    }
}