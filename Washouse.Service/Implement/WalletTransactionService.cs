using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class WalletTransactionService : IWalletTransactionService
    {

        private IWalletTransactionRepository _walletTransactionRepository ;
        public IUnitOfWork unitOfWork;

        public WalletTransactionService(IWalletTransactionRepository walletTransactionRepository, IUnitOfWork unitOfWork)
        {
            _walletTransactionRepository = walletTransactionRepository;
            this.unitOfWork = unitOfWork;
        }


        public async Task Add(WalletTransaction walletTransaction)
        {
            await _walletTransactionRepository.Add(walletTransaction);
        }

        public async Task Update(WalletTransaction walletTransaction)
        {
            await _walletTransactionRepository.Update(walletTransaction);
        }

        public IEnumerable<WalletTransaction> GetAll()
        {
            return _walletTransactionRepository.Get();
        }

        public async Task<WalletTransaction> GetById(int id)
        {
            return await _walletTransactionRepository.GetById(id);
        }
    }
}
