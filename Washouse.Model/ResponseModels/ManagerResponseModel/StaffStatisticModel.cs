using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class StaffStatisticModel
    {
        public OrderOverview orderOverview { get; set; }
        public ICollection<DailyStatistic> dailystatistics { get; set; }
    }
}
