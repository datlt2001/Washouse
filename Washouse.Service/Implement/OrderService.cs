using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels.ManagerResponseModel;
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
        IOrderDetailTrackingRepository _orderDetailTrackingRepository;
        IUnitOfWork _unitOfWork;

        public OrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository,
            IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork, 
            IPaymentRepository paymentRepository, IOrderTrackingRepository orderTrackingRepository, IOrderDetailTrackingRepository orderDetailTrackingRepository)
        {
            this._orderRepository = orderRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._deliveryRepository = deliveryRepository;
            this._unitOfWork = unitOfWork;
            this._paymentRepository = paymentRepository;
            this._orderTrackingRepository = orderTrackingRepository;
            this._orderDetailTrackingRepository = orderDetailTrackingRepository;
        }

        public async Task<Order> Create(Order order, List<OrderDetail> orderDetails, List<Delivery> deliveries, Payment payment, List<OrderTracking> orderTrackings)
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

                foreach (var orderTracking in orderTrackings)
                {
                    orderTracking.OrderId = order.Id;
                    await _orderTrackingRepository.Add(orderTracking);
                }

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

        public async Task Cancel(Order entity)
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
            foreach (var orderDetail in entity.OrderDetails)
            {
                orderDetail.Status = "Cancelled";
                var track = new OrderDetailTracking
                {
                    OrderDetailId = orderDetail.Id,
                    Status = "Cancelled",
                    CreatedBy = entity.UpdatedBy,
                    CreatedDate = DateTime.Now,
                };
                await _orderDetailTrackingRepository.Add(track);
                await _orderDetailRepository.Update(orderDetail);
            }
            foreach (var delivery in entity.Deliveries)
            {
                delivery.Status = "Cancelled";
                delivery.UpdatedDate = DateTime.Now;
                delivery.UpdatedBy = entity.UpdatedBy;
                await _deliveryRepository.Update(delivery);
            }
            var payment = entity.Payments.FirstOrDefault();
            payment.Status = "Cancelled";
            payment.UpdatedDate = DateTime.Now;
            payment.UpdatedBy = entity.UpdatedBy;
            await _paymentRepository.Update(payment);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _orderRepository.GetAll();
        }

        public async Task<IEnumerable<Order>> GetOrdersOfCustomer(int customerId, string customerMobile)
        {
            return await _orderRepository.GetOrdersOfCustomer(customerId, customerMobile);
        }

        public async Task<Order> GetAllOfDay(string date)
        {
            var order = await _orderRepository.GetAllOfDay(date);
            return order;
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
        
        public async Task<Order> GetOrderByIdCenterManaged(string id)
        {
            try
            {
                return await _orderRepository.GetOrderByIdCenterManaged(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public async Task<Order> GetOrderByIdToUpdateOrderDetail(string id)
        {
            try
            {
                return await _orderRepository.GetOrderByIdToUpdateOrderDetail(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Order> GetOrderWithPayment(string id)
        {
            try
            {
                return await _orderRepository.GetOrderWithPayment(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public async Task<Order> GetOrderWithDeliveries(string id)
        {
            try
            {
                return await _orderRepository.GetOrderWithDeliveries(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public async Task<Order> GetOrderWithDeliveriesAndPayment(string id)
        {
            try
            {
                return await _orderRepository.GetOrderWithDeliveriesAndPayment(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<StaffStatisticModel> GetStaffStatistics(int centerId)
        {
            try
            {
                return await _orderRepository.GetStaffStatistics(centerId);
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
