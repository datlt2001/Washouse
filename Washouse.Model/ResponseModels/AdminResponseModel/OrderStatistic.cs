using System.Collections.Generic;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class OrderStatistic
    {
        public int NumberOfNewOrderToday { get; set; }
        public int NumberOfNewOrderYesterday { get; set; }
        public List<Dictionary<string, int>> NumberOfNewOrderDaily { get; set; }
    }
}