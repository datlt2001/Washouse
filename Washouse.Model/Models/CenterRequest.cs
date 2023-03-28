using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class CenterRequest : Auditable
    {
        public int Id { get; set; }
        public int CenterRequesting { get; set; }
        public bool RequestStatus { get; set; }
        public string CenterName { get; set; }
        public string Alias { get; set; }
        public int LocationId { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string MonthOff { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public bool? HotFlag { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public bool HasDelivery { get; set; }

        public virtual Center CenterRequestingNavigation { get; set; }
    }
}
