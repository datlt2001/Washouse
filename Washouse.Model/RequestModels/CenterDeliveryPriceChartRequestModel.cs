namespace Washouse.Model.RequestModels
{
    public class CenterDeliveryPriceChartRequestModel
    {
        public decimal? MaxDistance { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
    }
}