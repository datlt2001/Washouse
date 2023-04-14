using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class DeliveryService : IDeliveryService
    {
        private IDeliveryRepository _deliveryRepository;
        public IUnitOfWork unitOfWork;

        public DeliveryService(IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork)
        {
            _deliveryRepository = deliveryRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task Update(Delivery delivery)
        {
            delivery.UpdatedDate = DateTime.Now;
           await _deliveryRepository.Update(delivery);
        }

        public async Task<Delivery> GetById(int id)
        {
            return await _deliveryRepository.GetById(id);
        }
    }
}
