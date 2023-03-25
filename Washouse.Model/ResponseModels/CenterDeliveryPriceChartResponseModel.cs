namespace Washouse.Model.ResponseModels
{
    public class CenterDeliveryPriceChartResponseModel
    {
        public int Id { get; set; }
        public decimal? MaxDistance { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal Price { get; set; }
    }
}