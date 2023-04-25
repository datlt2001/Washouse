using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public interface IFeedbackRepository : IRepository<Feedback>
    {
        public IEnumerable<int> GetIDList();
        public IEnumerable<Feedback> GetAllByCenterId(int id);
        public IEnumerable<Feedback> GetAllByOrderId(string orderId);
        public Task<Feedback> GetByOrderId(string orderId);
        public IEnumerable<Feedback> GetAllByServiceId(int serviceId);
        public Task<IEnumerable<Feedback>> GetAllByServiceIdLW(int serviceId);
        Task<IEnumerable<Feedback>> GetMyFeedback(string Email);

        Task<IEnumerable<Feedback>> GetAllByCenterIdAsync(int centerId);
    }
}