namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class CenterRequestModel
    {
        public bool RequestStatus { get; set; }
        public string CenterName { get; set; }
        public string Alias { get; set; }
        public int? WalletId { get; set; }
        public int LocationId { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string MonthOff { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public string TaxCode { get; set; }
        public string TaxRegistrationImage { get; set; }
        public bool? HotFlag { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public bool HasDelivery { get; set; }
    }
}