using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IWalletTransactionService
    {
        public Task Add(WalletTransaction walletTransaction);

        public Task Update(WalletTransaction walletTransaction);

        IEnumerable<WalletTransaction> GetAll();

        public Task<WalletTransaction> GetById(int id);
    }
}
