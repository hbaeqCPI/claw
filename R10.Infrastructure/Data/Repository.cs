using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data
{
    public class Repository<T> : RepositoryAsync<T>, IRepository<T> where T : class
    {
        public Repository(DbContext dbContext) : base(dbContext)
        {
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Add(params T[] entities)
        {
            _dbSet.AddRange(entities);
        }

        public void Add(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Delete(params T[] entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Delete(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public EntityEntry<T> Update(T entity)
        {
            return _dbSet.Update(entity);
        }

        public void Update(params T[] entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Update(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public EntityEntry<T> Attach(T entity)
        {
            return _dbSet.Attach(entity);
        }

        public void Attach(params T[] entities)
        {
            _dbSet.AttachRange(entities);
        }

        public void Attach(IEnumerable<T> entities)
        {
            _dbSet.AttachRange(entities);
        }
    }
}
