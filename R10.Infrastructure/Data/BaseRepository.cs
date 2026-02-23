using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R10.Infrastructure.Data
{
    public abstract class BaseRepository<T> : IRepositoryRead<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        public virtual IQueryable<T> QueryableList => _dbSet.AsNoTracking();

        public virtual IQueryable<T> FromSql(string sql)
        {
            return _dbSet.FromSqlRaw(sql).AsNoTracking();
        }

        public virtual IQueryable<T> FromSql(string sql, params object[] parameters)
        {
            return _dbSet.FromSqlRaw(sql, parameters).AsNoTracking();
        }
    }
}
