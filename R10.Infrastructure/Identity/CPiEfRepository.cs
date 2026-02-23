using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Identity
{

    public class CPiEfRepository<T> : IAsyncRepository<T> where T: class
    {
        protected readonly CPiUserDbContext _dbContext;

        public CPiEfRepository(CPiUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public IQueryable<T> QueryableList => _dbContext.Set<T>().AsNoTracking();

    }
}
