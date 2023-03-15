using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.Models
{
    public partial class DaysOfWeek
    {
        public DaysOfWeek()
        {
            OperatingHours = new HashSet<OperatingHour>();
        }

        public int Id { get; set; }
        public string DayName { get; set; }

        public virtual ICollection<OperatingHour> OperatingHours { get; set; }
    }
}
