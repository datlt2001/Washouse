using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Category : Auditable
    {
        public Category()
        {
            Services = new HashSet<Service>();
        }

        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Alias { get; set; }
        public int? ParentId { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public bool Status { get; set; }
        public bool HomeFlag { get; set; }

        public virtual ICollection<Service> Services { get; set; }
    }
}
