using System.ComponentModel.DataAnnotations;
using System;
using Washouse.Model.Models;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderDetailTrackingModel
    {
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        //public string CreatedBy { get; set; }
        public string UpdatedDate { get; set; }
        //public string UpdatedBy { get; set; }
    }
}