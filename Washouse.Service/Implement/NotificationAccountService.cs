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
    public class NotificationAccountService : INotificationAccountService
    {
        public INotificationAccountRepository _notificationAccountRepository;
        public IUnitOfWork _unitOfWork;

        public NotificationAccountService(INotificationAccountRepository notificationAccountRepository, IUnitOfWork unitOfWork)
        {
            _notificationAccountRepository = notificationAccountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Add(NotificationAccount notificationAccount)
        {
            await _notificationAccountRepository.Add(notificationAccount);
        }

        public IEnumerable<NotificationAccount> GetAll()
        {
            return _notificationAccountRepository.Get();
        }

        public async Task<NotificationAccount> GetById(int id)
        {
            return await _notificationAccountRepository.GetById(id);
        }

        public async Task Update(NotificationAccount notificationAccount)
        {
            await _notificationAccountRepository.Update(notificationAccount);
        }

        public NotificationAccount GetNotiAccbyNotiId(int notiId)
        {
            return _notificationAccountRepository.GetNotiAccbyNotiId(notiId);
        }
    }
}
