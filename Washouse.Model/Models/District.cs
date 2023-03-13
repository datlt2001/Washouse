using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class District
    {
        public District()
        {
            Wards = new HashSet<Ward>();
        }

        public int Id { get; set; }
        public string DistrictName { get; set; }

        public virtual ICollection<Ward> Wards { get; set; }
    }
}
