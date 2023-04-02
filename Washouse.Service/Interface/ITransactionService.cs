using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface ITransactionService 
    {
        public Task Add(Transaction transaction);

        public Task Update(Transaction transaction);

        IEnumerable<Transaction> GetAll();

        public Task<Transaction> GetById(int id);
    }
}
