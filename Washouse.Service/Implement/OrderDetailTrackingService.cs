using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Service.Interface;

namespace Washouse.Service.Implement
{
    public class OrderDetailTrackingService : IOrderDetailTrackingService
    {
        IOrderDetailTrackingRepository _orderDetailTrackingRepository;
        IOrderDetailRepository _orderDetailRepository;
        IUnitOfWork _unitOfWork;

        public OrderDetailTrackingService(IOrderDetailTrackingRepository orderDetailTrackingRepository, IOrderDetailRepository orderDetailRepository, IUnitOfWork unitOfWork)
        {
            this._orderDetailTrackingRepository = orderDetailTrackingRepository;
            this._orderDetailRepository = orderDetailRepository;
            this._unitOfWork = unitOfWork;
        }
        public async Task Add(OrderDetailTracking entity)
        {
            var orderDetail = await _orderDetailRepository.GetById(entity.OrderDetailId);
            orderDetail.Status = entity.Status;
            await _orderDetailTrackingRepository.Add(entity);
            await _orderDetailRepository.Update(orderDetail);
        }

        /*public DbSet<OrderDetailTracking> Get()
        {
            throw new NotImplementedException();
        }*/

        public Task Update(OrderDetailTracking entity)
        {
            throw new NotImplementedException();
        }
    }
}
