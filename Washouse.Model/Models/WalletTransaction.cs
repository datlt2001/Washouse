using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.Models
{
    public partial class WalletTransaction
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int FromWalletId { get; set; }
        public int ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime? UpdateTimeStamp { get; set; }

        public virtual Wallet FromWallet { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual Wallet ToWallet { get; set; }
    }
}
