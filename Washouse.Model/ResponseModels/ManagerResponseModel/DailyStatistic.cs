namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class DailyStatistic
    {
        public string Day { get; set; }
        public int TotalOrder { get; set; }
        public int SuccessfulOrder { get; set; }
        public int CancelledOrder { get; set; }
        public decimal Revenue { get; set; }
    }
}