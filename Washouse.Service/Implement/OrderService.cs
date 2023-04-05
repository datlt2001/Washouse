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
        IDeliveryRepository _deliveryRepository;
        IPaymentRepository _paymentRepository;
        IUnitOfWork _unitOfWork;

        public OrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository,
            IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork, IPaymentRepository paymentRepository)
        {
            this._orderRepository = orderRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._deliveryRepository = deliveryRepository;
            this._unitOfWork = unitOfWork;
            this._paymentRepository = paymentRepository;
        }

        public async Task<Order> Create(Order order, List<OrderDetail> orderDetails, List<Delivery> deliveries, Payment payment)
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
                foreach (var delivery in deliveries)
                {
                    delivery.OrderId = order.Id;
                    await _deliveryRepository.Add(delivery);
                }
                payment.OrderId = order.Id;
                await _paymentRepository.Add(payment);
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

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _orderRepository.GetAll();
        }

        public async Task<IEnumerable<Order>> GetAllOfDay(string date)
        {
            var orders = await _orderRepository.GetAll();
            return orders.Where(order => order.Id.StartsWith(date));
        }

        public async Task<IEnumerable<Order>> GetOrdersOfCenter(int centerId)
        {
            try
            {
                return await _orderRepository.GetOrdersOfCenter(centerId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
