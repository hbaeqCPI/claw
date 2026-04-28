using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiDbContext : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        IRepositoryAsync<TEntity> GetRepositoryAsync<TEntity>() where TEntity : class;
        IRepositoryReadAsync<TEntity> GetReadOnlyRepositoryAsync<TEntity>() where TEntity : class;
        IEntityFilterRepository GetEntityFilterRepository();

        int SaveChanges();
        Task<int> SaveChangesAsync();

        DbConnection GetDbConnection();
        DbSet<TEntity> Query<TEntity>() where TEntity : class;

        void Detach<TEntity>(TEntity entity) where TEntity : class;
        void Detach<TEntity>(List<TEntity> entities) where TEntity : class;
        void DetachAll();

        Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);
        Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default);
        Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default);
    }

    public interface ICPiDbContext<TContext> : ICPiDbContext where TContext : DbContext
    {
        TContext Context { get; }
    }
}