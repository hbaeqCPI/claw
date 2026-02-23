using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRepositoryReadAsync<T> : IRepositoryRead<T> where T : class
    {
        IProperty PrimaryKey { get; }

        /// <summary>
        /// Returns the surrogate key property of an entity with a natural primary key.
        /// Surrogate key property must be configured with UseSqlServerIdentityColumn annotation.
        /// </summary>
        IProperty SurrogateKey { get; }


        /// <summary>
        /// Returns entity with given primary key value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetByIdAsync(object id);

        /// <summary>
        /// Returns entity with given surrogate key value.
        /// If there is no surrogate key, given value will be treated as primary key value.
        /// Surrogate key property must be configured with UseSqlServerIdentityColumn annotation.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetByKeyAsync(int id);
    }
}
