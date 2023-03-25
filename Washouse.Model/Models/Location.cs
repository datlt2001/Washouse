using System;
using System.Collections.Generic;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Location
    {
        public Location()
        {
            Accounts = new HashSet<Account>();
            Centers = new HashSet<Center>();
            Orders = new HashSet<Order>();
            Customers = new HashSet<Customer>();
            Deliveries = new HashSet<Delivery>();
        }

        public int Id { get; set; }
        public string AddressString { get; set; }
        public int WardId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public virtual Ward Ward { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Center> Centers { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<Delivery> Deliveries { get; set; }
    }
}
