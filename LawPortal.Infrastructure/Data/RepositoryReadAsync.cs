using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Infrastructure.Data
{
    public class RepositoryReadAsync<T> : BaseRepository<T>, IRepositoryReadAsync<T> where T : class
    {
        public RepositoryReadAsync(DbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Returns the primary key property of an entity.
        /// </summary>
        public IProperty PrimaryKey => _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Single();

        /// <summary>
        /// Returns the surrogate key property of an entity with a natural primary key.
        /// Surrogate key property must be configured with UseSqlServerIdentityColumn annotation.
        /// </summary>
        //public IProperty SurrogateKey => _dbContext.Model.FindEntityType(typeof(T)).GetProperties().Where(p => !p.IsPrimaryKey() && p.SqlServer().ValueGenerationStrategy != null && p.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn).FirstOrDefault();
        public IProperty SurrogateKey => _dbContext.Model.FindEntityType(typeof(T)).GetProperties().Where(p => !p.IsPrimaryKey() && p.ValueGenerated !=null &&  p.GetValueGenerationStrategy()== SqlServerValueGenerationStrategy.IdentityColumn).FirstOrDefault();

        /// <summary>
        /// Returns entity with given primary key value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<T> GetByIdAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);

            //as no tracking
            _dbContext.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        /// <summary>
        /// Returns entity with given surrogate key value.
        /// If there is no surrogate key, given value will be treated as primary key value.
        /// Surrogate key property must be configured with UseSqlServerIdentityColumn annotation.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<T> GetByKeyAsync(int id)
        {
            if (SurrogateKey == null)
                return await GetByIdAsync(id);
            var keyName = SurrogateKey.Name;

            return await _dbSet.Where(e => EF.Property<int>(e, keyName) == id).SingleOrDefaultAsync();
        }
    }
}
