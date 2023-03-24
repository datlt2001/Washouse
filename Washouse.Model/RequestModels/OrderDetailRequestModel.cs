namespace Washouse.Model.RequestModels
{
    public class OrderDetailRequestModel
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string CustomerNote { get; set; }
        public string StaffNote { get; set; }
    }
}