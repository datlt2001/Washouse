using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels
{
    public class CategoryResponseModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryAlias { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public bool HomeFlag { get; set; }
    }
}
