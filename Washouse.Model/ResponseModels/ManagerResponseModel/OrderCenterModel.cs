﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderCenterModel
    {
        public string OrderId { get; set; }
        public string OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalOrderPayment { get; set; }
        public string Status { get; set; }
        public int DeliveryType { get; set; }
        public int? CenterId { get; set; }
        public string CenterName { get; set; }
        public bool IsFeedback { get; set; }
        public bool IsPayment { get; set; }
        public List<OrderedDeliveryModel> Deliveries { get; set; }
        public List<OrderedServiceModel> OrderedServices { get; set; }
    }
}
