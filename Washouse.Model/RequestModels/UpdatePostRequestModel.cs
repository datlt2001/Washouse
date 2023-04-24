using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class UpdatePostRequestModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public string SavedFileName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PublishTime { get; set; }
    }
}
