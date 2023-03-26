using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class ServiceRequest : Auditable
    {
        public int Id { get; set; }
        public int ServiceRequesting { get; set; }
        public bool RequestStatus { get; set; }
        public string ServiceName { get; set; }
        public string Alias { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public bool PriceType { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public decimal? MinPrice { get; set; }
        public string Unit { get; set; }
        public decimal Rate { get; set; }
        public int TimeEstimate { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public bool? HomeFlag { get; set; }
        public bool? HotFlag { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public int CenterId { get; set; }

        public virtual Service ServiceRequestingNavigation { get; set; }
    }
}
