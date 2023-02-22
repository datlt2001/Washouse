﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;

namespace Washouse.Service
{
    public class AccountService : IAccountService
    {
        public IAccountRepository _AccountRepository;
        private IUnitOfWork _unitOfWork;
        public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
        {
            _AccountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Add(Account account)
        {
            await _AccountRepository.Add(account);
        }

        public IEnumerable<Account> GetAll()
        {
            return _AccountRepository.Get();
        }

        public async Task<Account> GetById(int id)
        {
            return await _AccountRepository.GetById(id);
        }

        public async Task Update(Account account)
        {
            await _AccountRepository.Update(account);
        }

        public async Task ActivateAccount(int id)
        {
            await _AccountRepository.ActivateAccount(id);
        }

        public async Task DeactivateAccount(int id)
        {
            await _AccountRepository.DeactivateAccount(id);
        }

        public Account GetLoginAccount(string phone, string password)
        {
            return _AccountRepository.GetLoginAccount(phone, password);
        }

        public async Task ChangePassword(int id, string newPassword)
        {
            await _AccountRepository.ChangePassword(id, newPassword);
        }
    }
}
