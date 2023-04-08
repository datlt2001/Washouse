﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IOrderService
    {
        Task<Order> Create(Order order, List<OrderDetail> orderDetails, List<Delivery> deliveries, Payment payment, OrderTracking orderTracking);
        //void UpdateStatus(string orderId);
        Task Update(Order order);
        void Save();
        Task<IEnumerable<Order>> GetAll();
        Task<IEnumerable<Order>> GetAllOfDay(string date);
        Task<IEnumerable<Order>> GetOrdersOfCenter(int centerId);
        Task<Order> GetOrderById(string orderId);
    }
}
