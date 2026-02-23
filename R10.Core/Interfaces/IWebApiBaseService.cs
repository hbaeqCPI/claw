using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IWebApiBaseService<T, T1>
    {
        IQueryable<T1> QueryableList { get; }

        Task<int> Add(T webApiEntity, DateTime runDate);
        Task<List<int>> Import(List<T> webApiEntities, DateTime runDate);

        Task Update(int id, T webApiEntity, DateTime runDate);
        Task Update(List<T> webApiEntities, DateTime runDate);

        Task<bool> ValidatePermission(string systemId, List<string> roles, string respOffice);


        Task LogApiData(List<T> entities);
        Task LogApiData(T entity);

        /// <summary>
        /// Reset db entity states to clear db exceptions
        /// </summary>
        void ClearDbException();
    }
}
