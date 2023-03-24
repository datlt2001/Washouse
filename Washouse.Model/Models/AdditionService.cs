using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class AdditionService : Auditable
    {
        public int Id { get; set; }
        public string AdditionName { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Status { get; set; }
        public int CenterId { get; set; }

        public virtual Center Center { get; set; }
    }
}
