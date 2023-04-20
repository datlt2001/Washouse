using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IFeedbackService
    {
        public Task Add(Feedback feedback);

        public Task Update(Feedback feedback);

        IEnumerable<Feedback> GetAll();

        public Task<Feedback> GetById(int id);

        public IEnumerable<int> GetIDList();

        public IEnumerable<Feedback> GetAllByCenterId(int id);

        public IEnumerable<Feedback> GetAllByOrderId(string orderId);
        public IEnumerable<Feedback> GetAllByServiceId(int serviceId);
        public Task<IEnumerable<Feedback>> GetAllByServiceIdLW(int serviceId);

        public Task<IEnumerable<Feedback>> GetMyFeedback(string email);
    }
}