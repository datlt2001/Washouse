using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Abstract;

namespace Washouse.Model.Models
{
    [Table("Categories")]
    public class Category : Auditable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CategoryName { get; set; }
        public string Alias { get; set; }
        public int? ParentId { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        [Required]
        public bool Status { get; set; }
        public bool? HomeFlag { get; set; }
    }
}
