using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Microsoft.AspNetCore.Http;

namespace Washouse.Model.RequestModels
{
    public class CenterRequestModel
    {
        public string CenterName { get; set; }
        public string Alias { get; set; }
        //public int LocationId { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        [RegularExpression(@"^(0?[1-9]|[1-2][0-9]|3[0-1])(-(0?[1-9]|[1-2][0-9]|3[0-1]))*$", ErrorMessage = "Please insert a valid MonthOff. Formatted like 1-2-10-11-12-13")]
        public string? MonthOff { get; set; }
        public string? SavedFileName { get; set; }
        public string TaxCode { get; set; }
        public string TaxRegistrationImage { get; set; }
        //public bool? HasDelivery { get; set; }
    }
}
