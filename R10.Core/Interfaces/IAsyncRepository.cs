using R10.Core.Entities;
using R10.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IAsyncRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> GetByIdAsync(int id);
        IQueryable<T> QueryableList { get; }
      
        
    }
}
