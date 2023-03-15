using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Service : Auditable
    {
        public Service()
        {
            OrderDetails = new HashSet<OrderDetail>();
            ServiceGalleries = new HashSet<ServiceGallery>();
            ServicePrices = new HashSet<ServicePrice>();
            ServiceRequests = new HashSet<ServiceRequest>();
        }

        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string Alias { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public bool PriceType { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public int? TimeEstimate { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public bool? HomeFlag { get; set; }
        public bool? HotFlag { get; set; }
        public decimal Rating { get; set; }
        public int NumOfRating { get; set; }
        public int CenterId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Center Center { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ServiceGallery> ServiceGalleries { get; set; }
        public virtual ICollection<ServicePrice> ServicePrices { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
    }
}
