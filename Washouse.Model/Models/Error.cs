using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Washouse.Model.Models
{
    [Table("Errors")]
    public class Error
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
