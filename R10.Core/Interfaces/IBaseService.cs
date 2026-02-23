using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IBaseService<T>
    {
        IQueryable<T> QueryableList { get; }
        Task<T> GetByIdAsync(int entityId);

        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);

        Task Add(IEnumerable<T> entities);
        Task Update(IEnumerable<T> entities);
        Task Delete(IEnumerable<T> entities);

        Task UpdateRemarks(T entity);
    }
}
