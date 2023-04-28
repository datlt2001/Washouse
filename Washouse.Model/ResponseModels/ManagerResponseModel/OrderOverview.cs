namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderOverview
    {
        public int NumOfPendingOrder { get; set; } 
        public int NumOfProcessingOrder { get; set; } 
        public int NumOfReadyOrder { get; set; } 
        public int NumOfPendingDeliveryOrder { get; set; } 
        public int NumOfCompletedOrder { get; set; } 
        public int NumOfCancelledOrder { get; set; } 

    }
}