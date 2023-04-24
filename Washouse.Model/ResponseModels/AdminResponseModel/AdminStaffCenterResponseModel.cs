using System;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class AdminStaffCenterResponseModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public bool? IsManager { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? Gender { get; set; }
        public bool Status { get; set; }
        public string IdNumber { get; set; }
        public string IdFrontImg { get; set; }
        public string IdBackImg { get; set; }
    }
}