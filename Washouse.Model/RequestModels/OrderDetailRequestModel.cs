namespace Washouse.Model.RequestModels
{
    public class OrderDetailRequestModel
    {
        public string OrderId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}