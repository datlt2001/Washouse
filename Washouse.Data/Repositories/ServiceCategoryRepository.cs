using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;

namespace Washouse.Data.Repositories
{
    
    public class ServiceCategoryRepository : RepositoryBase<Category>, IServiceCategoryRepository
    {
        public ServiceCategoryRepository(IDbFactory dbFactory)
           : base(dbFactory)
        {
        }

        public async Task DeactivateCategory(int id)
        {
            try
            {
                
                var category = this.DbContext.Categories.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Categories.Attach(category);
                category.Status = false;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ActivateCategory(int id)
        {
            try
            {

                var category = this.DbContext.Categories.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Categories.Attach(category);
                category.Status = true;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task PinCategory(int id)
        {
            try
            {

                var category = this.DbContext.Categories.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Categories.Attach(category);
                category.HomeFlag = true;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task UnPinCategory(int id)
        {
            try
            {

                var category = this.DbContext.Categories.SingleOrDefault(c => c.Id.Equals(id));
                DbContext.Categories.Attach(category);
                category.HomeFlag = false;
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

}

