using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Center : Auditable
    {
        public Center()
        {
            AdditionServices = new HashSet<AdditionService>();
            CenterGalleries = new HashSet<CenterGallery>();
            CenterRequests = new HashSet<CenterRequest>();
            Feedbacks = new HashSet<Feedback>();
            Promotions = new HashSet<Promotion>();
            Resourses = new HashSet<Resourse>();
            Services = new HashSet<Service>();
            staff = new HashSet<Staff>();
        }

        public int Id { get; set; }
        public string CenterName { get; set; }
        public string Alias { get; set; }
        public int LocationId { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public string MonthOff { get; set; }
        public string WeekOff { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public bool? HotFlag { get; set; }
        public decimal? Rating { get; set; }

        public virtual Location Location { get; set; }
        public virtual ICollection<AdditionService> AdditionServices { get; set; }
        public virtual ICollection<CenterGallery> CenterGalleries { get; set; }
        public virtual ICollection<CenterRequest> CenterRequests { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<Resourse> Resourses { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<Staff> staff { get; set; }
    }
}
