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
    public class PaymentService : IPaymentService
    {
        IPaymentRepository _paymentRepository;
        IUnitOfWork _unitOfWork;

        public PaymentService(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Add(Payment payment)
        {
            await _paymentRepository.Add(payment);
        }

        public IEnumerable<Payment> GetAll()
        {
            return _paymentRepository.Get();
        }

        public async Task<Payment> GetById(int id)
        {
            return await _paymentRepository.GetById(id);
        }

        public async Task Update(Payment payment)
        {
            await _paymentRepository.Update(payment);
        }


    }
}
