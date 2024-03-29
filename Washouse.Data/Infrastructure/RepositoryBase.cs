﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var data = await _dbSet.ToListAsync();
            return data;
        }

        public async Task<TEntity> GetById(int id)
        {
            var data = await _dbSet.FindAsync(id);
            return data;
        }

        public async Task Add(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            _dbContext.SaveChanges();
        }

        public async Task Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            //return entity;
        }

        public async Task Delete(int id)
        {
            var entity = await GetById(id);
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteComplex(object firstKey, object secondKey)
        {
            var entity = await _dbSet.FindAsync(firstKey, secondKey);
            _dbSet.Remove(entity);
        }

        public virtual IEnumerable<TEntity> GetMulti(Expression<Func<TEntity, bool>> predicate, string[] includes = null)
        {
            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
            if (includes != null && includes.Count() > 0)
            {
                var query = _dbSet.Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                return query.Where<TEntity>(predicate).AsQueryable<TEntity>();
            }

            return _dbSet.Where<TEntity>(predicate).AsQueryable<TEntity>();
        }
    }
}
