using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Washouse.Model.Models;

namespace Washouse.Web.Models
{
    public class CategoryModel
    {
        public Category category { get; set; }
        public IFormFile Image { get; set; }      
    }
}
