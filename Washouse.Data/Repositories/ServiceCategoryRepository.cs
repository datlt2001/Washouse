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
        public IEnumerable<Category> GetAllParentCategory()
        {
            return this.DbContext.Categories.Where(x => x.ParentId == 0);
        }

        public IEnumerable<Category> GetCategoryByParentId(int id)
        {
            //var data = await _dbSet.FindAsync(id);
            return this.DbContext.Categories.Where(x => x.ParentId == id && x.Status == true);
            //return data;
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

    }

}

