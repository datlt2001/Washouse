﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    public class ServiceRepository : RepositoryBase<Service>, IServiceRepository
    {
        public ServiceRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }

        public async Task DeactivateService(int id)
        {
            try
            {
                var service = this.DbContext.Services.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Services.Attach(service);
                service.Status = "unactive";
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public new async Task<Service> GetById(int id)
        {
            var data = await this._dbContext.Services
                    .Include(service => service.OrderDetails)
                    .Include(service => service.ServiceGalleries)
                    .Include(service => service.ServicePrices)
                    .Include(service => service.Center)
                    .Include(service => service.Category)
                    .FirstOrDefaultAsync(service => service.Id == id);
            return data;
        }
    }
}
