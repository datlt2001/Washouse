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
        public string MonthOff { get; set; }
        public string? SavedFileName { get; set; }
    }
}
