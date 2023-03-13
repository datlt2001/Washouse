using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Staff : Auditable
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public bool Status { get; set; }
        public bool IsManager { get; set; }
        public int CenterId { get; set; }
        public string IdNumber { get; set; }
        public string IdFrontImg { get; set; }
        public string IdBackImg { get; set; }

        public virtual Account Account { get; set; }
        public virtual Center Center { get; set; }
    }
}
