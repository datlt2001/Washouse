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
    public class WalletService :IWalletService
    {
        private IWalletRepository _walletRepository;
        public IUnitOfWork unitOfWork;

        public WalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
        {
            _walletRepository = walletRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Add(Wallet wallet)
        {
            await _walletRepository.Add(wallet);
        }

        public async Task Update(Wallet wallet)
        {
            await _walletRepository.Update(wallet);
        }

        public IEnumerable<Wallet> GetAll()
        {
            return _walletRepository.Get();
        }

        public async Task<Wallet> GetById(int id)
        {
            return await _walletRepository.GetById(id);
        }
    }
}
