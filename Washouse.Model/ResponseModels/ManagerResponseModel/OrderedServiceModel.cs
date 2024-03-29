﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Model.ResponseModels.ManagerResponseModel
{
    public class OrderedServiceModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategory { get; set; }
        public decimal? Measurement { get; set; }
        public string Unit { get; set; }
        public string Image { get; set; }
        public decimal? Price { get; set; }
    }
}
