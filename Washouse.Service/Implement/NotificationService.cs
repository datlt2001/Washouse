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
    public class NotificationService : INotificationService
    {
        public INotificationRepository _notificationRepository;
        public IUnitOfWork unitOfWork;

        public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Add(Notification notification)
        {
            await _notificationRepository.Add(notification);
        }

        public IEnumerable<Notification> GetAll()
        {
            return _notificationRepository.Get();
        }

        public async Task<Notification> GetById(int id)
        {
            return await _notificationRepository.GetById(id);
        }

        public async Task Update(Notification notification)
        {
            await _notificationRepository.Update(notification);
        }
    }
}
