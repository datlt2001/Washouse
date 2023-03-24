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
    public class OrderService : IOrderService
    {
        IOrderRepository _orderRepository;
        IOrderDetailRepository _orderDetailRepository;
        IOrderAdditionRepository _orderAdditionRepository;
        IUnitOfWork _unitOfWork;

        public OrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IOrderAdditionRepository orderAdditionRepository, IUnitOfWork unitOfWork)
        {
            this._orderRepository = orderRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._orderAdditionRepository = orderAdditionRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task<Order> Create(Order order, List<OrderDetail> orderDetails, List<OrderAddition> orderAdditions)
        {
            try
            {
                await _orderRepository.Add(order);
                _unitOfWork.Commit();

                foreach (var orderDetail in orderDetails)
                {
                    orderDetail.OrderId = order.Id;
                    await _orderDetailRepository.Add(orderDetail);
                }

                foreach (var orderAddition in orderAdditions)
                {
                    orderAddition.OrderId = order.Id;
                    await _orderAdditionRepository.Add(orderAddition);
                }

                return order;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async void UpdateStatus(int orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            order.Status = "Pending";
            await _orderRepository.Update(order);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

    }
}
