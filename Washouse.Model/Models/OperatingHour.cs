using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Abstract;

namespace Washouse.Model.Models
{
    public partial class OperatingHour : Auditable
    {
        public int CenterId { get; set; }
        public int DaysOfWeekId { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        public virtual Center Center { get; set; }
        public virtual DaysOfWeek DaysOfWeek { get; set; }
    }
}
