﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class ServicePriceRepository : RepositoryBase<ServicePrice>, IServicePriceRepository
    {
        public ServicePriceRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}
