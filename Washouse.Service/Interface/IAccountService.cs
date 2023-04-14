using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IAccountService
    {
        public Task Add(Account account);

        public Task Update(Account account);

        Task<IEnumerable<Account>> GetAll();

        public Task<Account> GetById(int id);

        public Task ActivateAccount(int id);

        public Task DeactivateAccount(int id);

        Account GetLoginAccount(string phone, string password);

        public Task ChangePassword(int id, string newPassword);

        public Account GetAccountByPhone(string phone);

        public Account GetAccountByEmail(string email);

        Account GetAccountByEmailAndPhone(string email, string phone);
    }
}