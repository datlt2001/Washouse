using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.RequestModels;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class AdminStatisticResponseModel
    {
        public CenterStatistic centerOverview { get; set; }
        public List<AdminDailyStatistic> dailystatistics { get; set; }
        //public OrderStatistic OrderStatistic { get; set; }
        //public CustomerStatistic CustomerStatistic { get; set; }
        //public PostStatistic PostStatistic { get; set; }
    }
}
