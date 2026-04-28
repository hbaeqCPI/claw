using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LawPortal.Infrastructure.Data
{
    public class CPiDbContext<TContext> : IRepositoryFactory, ICPiDbContext<TContext>, ICPiDbContext where TContext : DbContext, IDisposable
    {
        private ConcurrentDictionary<Type, object> _repositories;
        private ConcurrentDictionary<Type, object> _readOnlyRepositories;
        private IEntityFilterRepository _entityFilterRepository;
        public TContext Context { get; }

        public CPiDbContext(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public DbConnection GetDbConnection()
        {
            return Context.Database.GetDbConnection();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories == null) _repositories = new ConcurrentDictionary<Type, object>();

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type)) _repositories[type] = new Repository<TEntity>(Context);
            return (IRepository<TEntity>)_repositories[type];
        }

        /// <summary>
        /// AddAsync creates additional overhead
        /// Always use non async method except when deleting using id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IRepositoryAsync<TEntity> GetRepositoryAsync<TEntity>() where TEntity : class
        {
            if (_repositories == null) _repositories = new ConcurrentDictionary<Type, object>();

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type)) _repositories[type] = new RepositoryAsync<TEntity>(Context);
            return (IRepositoryAsync<TEntity>)_repositories[type];
        }

        public IRepositoryReadAsync<TEntity> GetReadOnlyRepositoryAsync<TEntity>() where TEntity : class
        {
            if (_readOnlyRepositories == null) _readOnlyRepositories = new ConcurrentDictionary<Type, object>();

            var type = typeof(TEntity);
            if (!_readOnlyRepositories.ContainsKey(type)) _readOnlyRepositories[type] = new RepositoryReadAsync<TEntity>(Context);
            return (IRepositoryReadAsync<TEntity>)_readOnlyRepositories[type];
        }

        public IEntityFilterRepository GetEntityFilterRepository()
        {
            if (_entityFilterRepository == null) _entityFilterRepository = new EntityFilterRepository(Context);
            return _entityFilterRepository;
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public DbSet<T> Query<T>() where T : class
        {
            return Context.Set<T>();
        }

        public void Detach<T>(T entity) where T : class
        {
            Context.Entry(entity).State = EntityState.Detached;
        }

        public void Detach<T>(List<T> entities) where T : class
        {
            entities.ForEach(entity => { Detach(entity); });
        }

        public void DetachAll()
        {
            var entries = Context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
                entry.State = EntityState.Detached;
        }

        public async Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default)
        {
            return await Context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        public async Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default)
        {
            return await Context.Database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }

        public async Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default)
        {
            return await Context.Database.ExecuteSqlAsync(sql, cancellationToken);
        }
    }
}
