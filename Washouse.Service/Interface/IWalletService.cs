using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IWalletService
    {
        public Task Add(Wallet wallet);

        public Task Update(Wallet wallet);

        IEnumerable<Wallet> GetAll();

        public Task<Wallet> GetById(int id);
    }
}
