namespace Washouse.Model.RequestModels
{
    public class OrderDetailRequestModel
    {
        public int ServiceId { get; set; }
        public decimal Measurement { get; set; }
        public decimal Price { get; set; }
        public string CustomerNote { get; set; }
        public string StaffNote { get; set; }
    }
}