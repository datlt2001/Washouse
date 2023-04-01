using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Washouse.Model.Abstract;

namespace Washouse.Model.Models
{
    public partial class Wallet : Auditable
    {
        public Wallet()
        {
            Accounts = new HashSet<Account>();
            Centers = new HashSet<Center>();
            Transactions = new HashSet<Transaction>();
            WalletTransactionFromWallets = new HashSet<WalletTransaction>();
            WalletTransactionToWallets = new HashSet<WalletTransaction>();
        }

        public int Id { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }


        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Center> Centers { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactionFromWallets { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactionToWallets { get; set; }
    }
}
