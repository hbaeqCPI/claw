using R10.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data
{
    public class EfPatentRepository<T> : IAsyncRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _dbContext;

        public EfPatentRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public IQueryable<T> QueryableList => _dbContext.Set<T>().AsNoTracking();

        //public async Task<List<T>> ListAllAsync()
        //{
        //    return await _dbContext.Set<T>().ToListAsync();
        //}



    }
}
