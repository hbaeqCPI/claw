using Microsoft.EntityFrameworkCore;

namespace R10.Core.Interfaces
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>() where T : class;
        IRepositoryAsync<T> GetRepositoryAsync<T>() where T : class;
        IRepositoryReadAsync<T> GetReadOnlyRepositoryAsync<T>() where T : class;
        IEntityFilterRepository GetEntityFilterRepository();
        DbSet<T> Query<T>() where T : class;
    }
}
