using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class ServiceGallery
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Image { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Service Service { get; set; }
    }
}
