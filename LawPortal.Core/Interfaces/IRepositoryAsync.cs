using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IRepositoryAsync<T> : IRepositoryReadAsync<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddAsync(params T[] entities);
        Task AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        Task DeleteAsync(object id);

        Task UpdateAsync(T entity);

        Task<int> UpdateKeyAsync(T entity, string keyColumnName, string idName, object keyValue, object idValue,string? updatedBy="");
    }
}
