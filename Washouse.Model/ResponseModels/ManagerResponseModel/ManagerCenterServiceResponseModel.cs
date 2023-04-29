using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class ManagerCenterServiceResponseModel
    {
        public ManagerCenterServiceResponseModel()
        {
            Services = new HashSet<MyCenterServiceResponseModel>();
        }
        public int ServiceCategoryID { get; set; }
        public string ServiceCategoryName { get; set; }
        public virtual ICollection<MyCenterServiceResponseModel> Services { get; set; }
    }
}
