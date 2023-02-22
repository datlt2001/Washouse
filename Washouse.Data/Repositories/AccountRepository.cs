using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task DeactivateAccount(int id)
        {
            try
            {

                var account = this.DbContext.Accounts.SingleOrDefault(a => a.Id.Equals(id));
                DbContext.Accounts.Attach(account);
                account.Status = false;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ActivateAccount(int id)
        {
            try
            {

                var account = this.DbContext.Accounts.SingleOrDefault(a => a.Id.Equals(id));
                DbContext.Accounts.Attach(account);
                account.Status = true;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public  Account GetLoginAccount(string phone, string password)
        {
            try
            {

                return this.DbContext.Accounts.SingleOrDefault(a => a.Phone.Equals(phone) && a.Password.Equals(password));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ChangePassword(int id, string newPass)
        {
            try
            {

                var account = this.DbContext.Accounts.SingleOrDefault(a => a.Id.Equals(id));
                DbContext.Accounts.Attach(account);
                account.Password = newPass;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
