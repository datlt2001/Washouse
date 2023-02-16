using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Washouse.Data.Infrastructure
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public readonly DbSet<TEntity> _dbSet;
        public WashouseDbContext _dbContext;

        protected IDbFactory DbFactory = new DbFactory();
        /*protected IDbFactory DbFactory
        {
            get;
            private set;
        }*/

        protected WashouseDbContext DbContext
        {
            get { return _dbContext ?? (_dbContext = this.DbFactory.Init()); }
            //get { return _dbContext ?? (_dbContext = new WashouseDbContext()); }
        }
        public RepositoryBase(IDbFactory dbFactory)
        {
            _dbSet = DbContext.Set<TEntity>();
            DbFactory = dbFactory;
        }

        public DbSet<TEntity> Get()
        {
            return _dbSet;
        }

        public async Task<TEntity> GetById(long id)
        {
            var data = await _dbSet.FindAsync(id);
            return data;
        }

        public async Task Add(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public async Task Delete(long id)
        {
            var entity = await GetById(id);
            _dbSet.Remove(entity);
        }
        public async Task DeleteComplex(object firstKey, object secondKey)
        {
            var entity = await _dbSet.FindAsync(firstKey, secondKey);
            _dbSet.Remove(entity);
        }
    }
}
