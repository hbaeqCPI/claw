using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class LoggerService<T, TContext> : ILoggerService<T, TContext> where T : class where TContext : DbContext
    {
        protected readonly DbSet<T> _dbSet;
        public TContext Context { get; }

        public LoggerService(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = Context.Set<T>();
        }

        public virtual IQueryable<T> QueryableList => _dbSet.AsNoTracking();

        public virtual async Task<T> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);

            //as no tracking
            Context.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        public virtual async Task Add(T entity)
        {
            _dbSet.Add(entity);
            await Context.SaveChangesAsync();
        }
    }
}
