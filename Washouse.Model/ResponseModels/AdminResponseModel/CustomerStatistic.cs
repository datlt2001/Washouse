using System.Collections.Generic;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class CustomerStatistic
    {
        public int NumberOfNewCustomersToday { get; set; }
        public int NumberOfNewCustomersYesterday { get; set; }
        public List<Dictionary<string, int>> NumberOfNewCustomersDaily { get; set; }
    }
}