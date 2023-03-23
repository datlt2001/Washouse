using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class ResourceRequestModel
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public int Quantity { get; set; }
        public decimal? WashCapacity { get; set; }
        public decimal? DryCapacity { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
