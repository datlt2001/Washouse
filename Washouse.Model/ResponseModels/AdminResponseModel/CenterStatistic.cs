using System.Collections.Generic;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class CenterStatistic
    {
        //public int NumberOfNewCenterToday { get; set; }
        //public int NumberOfNewCenterYesterday { get; set; }
        public int NumberOfPendingCenters { get; set; }
        public int NumberOfActiveCenters { get; set; }
        public int NumberOfClosedCenters { get; set; }
    }
}