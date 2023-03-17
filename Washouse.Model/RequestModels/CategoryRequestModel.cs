using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.RequestModels
{
    public class CategoryRequestModel
    {
        public string CategoryName { get; set; }
        public string Alias { get; set; }
        public int? ParentId { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public bool HomeFlag { get; set; }
        public IFormFile? Photo { get; set; }
        public string? SavedUrl { get; set; }
        public string? SignedUrl { get; set; }
        public string? SavedFileName { get; set; }
    }
}
