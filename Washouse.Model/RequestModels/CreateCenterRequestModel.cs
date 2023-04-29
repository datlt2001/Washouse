using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels;

namespace Washouse.Model.RequestModels
{
    public class CreateCenterRequestModel
    {
        public CenterRequestModel Center { get; set; }
        public LocationRequestModel Location { get; set; }
        public ICollection<OperatingHoursRequestModel> CenterOperatingHours { get; set; }
        public CenterDeliveryRequestModel CenterDelivery { get; set; }
        //public ICollection<ResourceRequestModel>? Resources { get; set; }
    }
}
