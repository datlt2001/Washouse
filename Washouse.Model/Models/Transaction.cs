using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }

        public virtual Wallet Wallet { get; set; }
    }
}
