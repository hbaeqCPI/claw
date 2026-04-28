using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IRepository<T> : IRepositoryAsync<T>, IRepositoryReadAsync<T> where T : class
    {
        void Add(T entity);
        void Add(params T[] entities);
        void Add(IEnumerable<T> entities);

        void Delete(T entity);
        void Delete(params T[] entities);
        void Delete(IEnumerable<T> entities);

        EntityEntry<T> Update(T entity);
        void Update(params T[] entities);
        void Update(IEnumerable<T> entities);

        EntityEntry<T> Attach(T entity);
        void Attach(params T[] entities);
        void Attach(IEnumerable<T> entities);
    }
}
