using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Data.Infrastructure
{
    public interface IRepository<TEntity> where TEntity : class
    {
        DbSet<TEntity> Get();

        Task<TEntity> GetById(int id);

        Task Add(TEntity entity);

        Task Update(TEntity entity);

        Task Delete(int id);
        Task DeleteComplex(object firstKey, object secondKey);

        IEnumerable<TEntity> GetMulti(Expression<Func<TEntity, bool>> predicate, string[] includes = null);
    }
}
