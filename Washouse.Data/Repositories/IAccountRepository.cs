using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        public Task ActivateAccount(int id);

        public Task DeactivateAccount(int id);

        Account GetLoginAccount(string phone, string password);

        public Task ChangePassword(int id, string newPassword);
        public Account GetAccountByPhone(string phone);
        public Account GetAccountByEmail(string email);
        public Task<Account> GetAccountByEmailAsync(string email);

        Account GetAccountByEmailAndPhone(string email, string phone);
        Task<Account> GetByIdLightWeight(int id);
    }
}
