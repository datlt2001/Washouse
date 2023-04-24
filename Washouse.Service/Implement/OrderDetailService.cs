using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class OrderDetailService : IOrderDetailService
    {
        IOrderDetailRepository _orderDetailRepository;
        IUnitOfWork _unitOfWork;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository, IUnitOfWork unitOfWork)
        {
            _orderDetailRepository = orderDetailRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderDetail>> GetByOrderId(string orderId)
        {
            return await _orderDetailRepository.GetByOrderId(orderId);
        }
    }
}