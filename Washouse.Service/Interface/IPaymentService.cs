using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IPaymentService
    {
        public Task Add(Payment payment);

        public Task Update(Payment payment);

        IEnumerable<Payment> GetAll();

        public Task<Payment> GetById(int id);
    }
}
