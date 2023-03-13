using System;
using System.Collections.Generic;
using Washouse.Model.Abstract;

#nullable disable

namespace Washouse.Model.Models
{
    public partial class Customer : Auditable
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public int? AccountId { get; set; }
        public bool Status { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public int? Address { get; set; }
        public string Email { get; set; }

        public virtual Account Account { get; set; }
        public virtual Location AddressNavigation { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
