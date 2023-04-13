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
        IOrderTrackingRepository _orderTrackingRepository;
        IUnitOfWork _unitOfWork;

        public OrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository,
            IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork, 
            IPaymentRepository paymentRepository, IOrderTrackingRepository orderTrackingRepository)
        {
            this._orderRepository = orderRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._deliveryRepository = deliveryRepository;
            this._unitOfWork = unitOfWork;
            this._paymentRepository = paymentRepository;
            this._orderTrackingRepository = orderTrackingRepository;
        }

        public async Task<Order> Create(Order order, List<OrderDetail> orderDetails, List<Delivery> deliveries, Payment payment, OrderTracking orderTracking)
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

                orderTracking.OrderId = order.Id;
                await _orderTrackingRepository.Add(orderTracking);

                return order;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task Update(Order entity)
        {
            var orderTracking = new OrderTracking
            {
                OrderId = entity.Id,
                Status = entity.Status,
                CreatedBy = entity.UpdatedBy,
                CreatedDate = DateTime.Now,
            };
            await _orderTrackingRepository.Add(orderTracking);
            await _orderRepository.Update(entity);
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

        public async Task<Order> GetOrderById(string id)
        {
            try
            {
                return await _orderRepository.GetOrderById(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateOrderDetail(OrderDetail orderDetail, Payment payment)
        {
            try
            {
                await _paymentRepository.Update(payment);
                await _orderDetailRepository.Update(orderDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
