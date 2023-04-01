using System;
using System.ComponentModel.DataAnnotations;

namespace Washouse.Model.RequestModels
{
    public class OrderRequestModel
    {
        public string CustomerName { get; set; }
        public string CustomerAddressString { get; set; }
        public int CustomerWardId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerMessage { get; set; }
        public int CustomerId { get; set; }
        [Required]
        [Range(0, 3, ErrorMessage = "Please enter a value from 0 to 3")]
        public int DeliveryType { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public string PreferredDropoffTime { get; set; }
        public string PreferredDeliverTime { get; set; }
    }
}