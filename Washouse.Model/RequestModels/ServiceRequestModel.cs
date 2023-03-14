using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.RequestModels
{
    public class ServiceRequestModel
    {
        public string ServiceName { get; set; }
        public string Alias { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public bool PriceType { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
        public int TimeEstimate { get; set; }
        public int CenterId { get; set; }
    }
}
