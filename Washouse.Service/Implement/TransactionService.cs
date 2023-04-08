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
    public class TransactionService : ITransactionService
    {
        private ITransactionRepository _transactionRepository;
        private IWalletRepository _walletRepository;
        public IUnitOfWork unitOfWork;

        public TransactionService(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, IWalletRepository walletRepository)
        {
            _transactionRepository = transactionRepository;
            this.unitOfWork = unitOfWork;
            _walletRepository= walletRepository;
        }

        public async Task Add(Transaction transaction)
        {
            await _transactionRepository.Add(transaction);
            
            if(transaction.Type == "deposit")
            {
                Wallet wallet = await _walletRepository.GetById(transaction.WalletId);
                wallet.Balance = wallet.Balance + transaction.Amount;
                await _walletRepository.Update(wallet);
            }          
        }

        public async Task Update(Transaction transaction)
        {
            await _transactionRepository.Update(transaction);
        }

        public IEnumerable<Transaction> GetAll()
        {
            return _transactionRepository.Get();
        }

        public async Task<Transaction> GetById(int id)
        {
            return await _transactionRepository.GetById(id);
        }
    }
}
