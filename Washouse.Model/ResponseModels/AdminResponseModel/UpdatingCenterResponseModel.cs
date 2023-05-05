using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class UpdatingCenterResponseModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        public string CenterAddress { get; set; }
        public string CenterPhone { get; set; }
        public int LocationId { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public bool HasDelivery { get; set; }
        public bool HasOnlinePayment { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; }
        public string TaxCode { get; set; }
        public string TaxRegistrationImage { get; set; }
        public string LastDeactivate { get; set; }
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; }
        public string ManagerEmail { get; set; }
        public string ManagerPhone { get; set; }
        public int[] Ratings { get; set; }
    }
}
