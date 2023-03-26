using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface INotificationService
    {
        public Task Add(Notification notification);

        IEnumerable<Notification> GetAll();

        public Task Update(Notification notification);


        public Task<Notification> GetById(int id);
    }
}
