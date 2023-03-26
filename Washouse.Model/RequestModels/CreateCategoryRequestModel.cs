using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CreateCategoryRequestModel
    {
        public string CategoryName { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public bool HomeFlag { get; set; }
        public string? SavedFileName { get; set; }
    }
}
