using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Model.ResponseModels
{
    public class WalletResponseModel
    {
        public int WalletId { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }
        public virtual ICollection<TransactionResponseModel> Transactions { get; set; }
        public virtual ICollection<WalletTransactionResponseModel> WalletTransactions { get; set; }
    }
}
