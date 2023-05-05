using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class AdminDailyStatistic
    {
        public string Day { get; set; }
        public int NumberOfNewPosts { get; set; }
        public int NumberOfNewUsers { get; set; }
        public int NumberOfNewCenters { get; set; }
    }
}
