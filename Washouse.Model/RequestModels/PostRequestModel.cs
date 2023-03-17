using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class PostRequestModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile Thumbnail { get; set; }
        public string Type { get; set; }
    }
}
