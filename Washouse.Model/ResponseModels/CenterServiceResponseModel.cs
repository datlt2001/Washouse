using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CenterServiceResponseModel
    {
        public CenterServiceResponseModel() {
            Services = new HashSet<ServicesOfCenterResponseModel>();
        }
        public int ServiceCategoryID { get; set;}
        public string ServiceCategoryName { get; set;}
        public virtual ICollection<ServicesOfCenterResponseModel> Services { get; set; }
    }
}
