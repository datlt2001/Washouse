using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.RequestModels
{
    public class OperatingHoursRequestModel
    {
        public int Day { get; set; }

        [RegularExpression(@"([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Please insert a valid Opentime. Formatted like 07:15")]
        public string OpenTime { get; set; }
        [RegularExpression(@"([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Please insert a valid Opentime. Formatted like 19:15")]
        public string CloseTime { get; set; }
    }
}
