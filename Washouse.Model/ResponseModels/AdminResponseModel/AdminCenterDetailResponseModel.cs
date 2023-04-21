using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.ResponseModels.ManagerResponseModel;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class AdminCenterDetailResponseModel
    {
        public AdminCenterDetailResponseModel()
        {
            Staffs = new HashSet<AdminStaffCenterResponseModel>();
            Services = new HashSet<AdminServiceCenterModel>();
            Feedbacks = new HashSet<AdminFeedbackCenterModel>();
        }
        public AdminCenterBasicResponseModel Center { get; set; }
        public virtual ICollection<AdminStaffCenterResponseModel> Staffs { get; set; }
        public virtual ICollection<AdminServiceCenterModel> Services { get; set; }
        public virtual ICollection<AdminFeedbackCenterModel> Feedbacks { get; set; }
    }
}
