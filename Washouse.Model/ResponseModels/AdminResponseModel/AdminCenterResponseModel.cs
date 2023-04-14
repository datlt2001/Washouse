using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.AdminResponseModel
{
    public class AdminCenterResponseModel
    {
        public int Id { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public decimal? Rating { get; set; }
        public int NumOfRating { get; set; }
        public string Phone { get; set; }
        public string CenterAddress { get; set; }
        //public bool MonthOff { get; set; }
        //public bool IsOpening { get; set; }
        public string Status { get; set; }
        public string TaxCode { get; set; }
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; }

    }
}
