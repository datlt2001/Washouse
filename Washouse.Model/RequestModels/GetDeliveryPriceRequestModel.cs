using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class GetDeliveryPriceRequestModel
    {
        public int CenterId { get; set; }
        public decimal TotalWeight { get; set; }
        public string CustomerAddressString { get; set; }    
        public int CustomerWardId { get; set; }
        [Required]
        [Range(0, 3, ErrorMessage = "Please enter a value from 0 to 3")]
        public int DeliveryType { get; set; }    
    }
}
