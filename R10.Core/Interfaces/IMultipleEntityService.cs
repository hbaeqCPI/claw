using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IMultipleEntityService<T1, T2> : IChildEntityService<T1, T2>
    {
        IQueryable<T2> QueryableListWithEntityFilter { get; }

        Task Reorder(int id, string userName, int newIndex);

        Task RefreshOrderOfEntry(int parentId);
    }

    public interface IMultipleEntityService<T> where T:BaseEntity
    {
        IQueryable<T> QueryableListWithEntityFilter { get; }
        Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted);
        Task Reorder(int id, string userName, int newIndex);
        
    }
}
