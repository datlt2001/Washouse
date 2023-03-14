using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.ResponseModels
{
    public class CenterResponseModel
    {
        public CenterResponseModel()
        {
            CenterServices = new HashSet<CenterServiceResponseModel>();
        }
        public int CenterId { get; set; }
        public string CenterName { get; set; }
        public string Alias { get; set; }
        public string CenterAddress { get; set; }
        public CenterLocationResponseModel CenterLocation { get; set; }
        public CenterOperatingHoursResponseModel CenterOperatingHours { get; set; }
        public string CenterPhone { get; set; }
        public virtual ICollection<CenterServiceResponseModel> CenterServices { get; set; }
        public decimal CenterRating { get; set; }
        public int CenterRatingCount { get; set; }
    }
}
