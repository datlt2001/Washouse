using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class DriverInformationRequestModel
    {
        [Required]
        public string ShipperName { get; set; }
        [Required]
        public string ShipperPhone { get; set; }
    }
}
