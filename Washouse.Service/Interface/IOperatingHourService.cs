﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Service.Interface
{
    public interface IOperatingHourService
    {
        Task Add(OperatingHour operatingHour);
        Task Update(OperatingHour operatingHour);
    }
}
