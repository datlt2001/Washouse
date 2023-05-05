namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class StatisticOfAllCentersDaily
    {
        public string Day { get; set; }
        public int NumberOfPendingCenters { get; set; }
        public int NumberOfActiveCenters { get; set; }
        public int NumberOfClosedCenters { get; set; }
    }
}