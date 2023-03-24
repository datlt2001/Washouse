namespace Washouse.Model.RequestModels
{
    public class OrderRequestModel
    {
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerMessage { get; set; }
        public int CustomerId { get; set; }
    }
}