namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class DailyStatistic
    {
        public string Day { get; set; }
        public int SuccessfulOrder { get; set; }
        public int CancelledOrder { get; set; }
    }
}