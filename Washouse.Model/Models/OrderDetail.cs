using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Washouse.Model.Models
{
    [Table("OrderDetails")]
    public class OrderDetail
    {
        //[Key]
        //[Column(Order = 1)]
        public int OrderId { get; set; }

        //[Key]
        //[Column(Order = 2)]
        public int ServiceId { get; set; }

        public int Quantity { set; get; }

        public decimal Price { set; get; }

        [ForeignKey("OrderId")]

        public virtual Order Order { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
}
