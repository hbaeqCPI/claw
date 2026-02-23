using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data
{
    public class RepositoryAsync<T> : RepositoryReadAsync<T>, IRepositoryAsync<T> where T : class
    {
        public RepositoryAsync(DbContext dbContext) : base(dbContext)
        {
            
        }

        /// <summary>
        /// This method is async only to allow special value generators, such as the one used by 'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo', to access the database asynchronously. For all other cases the non async method should be used.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        /// <summary>
        /// This method is async only to allow special value generators, such as the one used by 'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo', to access the database asynchronously. For all other cases the non async method should be used.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task AddAsync(params T[] entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// This method is async only to allow special value generators, such as the one used by 'Microsoft.EntityFrameworkCore.Metadata.SqlServerValueGenerationStrategy.SequenceHiLo', to access the database asynchronously. For all other cases the non async method should be used.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public async Task DeleteAsync(object id)
        {
            var typeInfo = typeof(T).GetTypeInfo();
            var key = _dbContext.Model.FindEntityType(typeInfo).FindPrimaryKey().Properties.FirstOrDefault();
            var property = typeInfo.GetProperty(key?.Name);
            if (property != null)
            {
                var entity = Activator.CreateInstance<T>();
                property.SetValue(entity, id);
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity != null) _dbSet.Remove(entity);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            if (SurrogateKey != null)
            {
                var idName = SurrogateKey.Name;
                var idValue = entity.GetType().GetProperty(idName).GetValue(entity, null);
                //var keyColumnName = _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(x => x.SqlServer()).Single().ColumnName;
                var keyColumnName = _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(x => x.GetColumnName()).FirstOrDefault();
                var keyPropertyName = _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(x => x.Name).Single();

                var keyValue = entity.GetType().GetProperty(keyPropertyName).GetValue(entity, null);

                await UpdateKeyAsync(entity, keyColumnName, idName, keyValue, idValue);
            }

            _dbSet.Update(entity);
        }

        public async Task<int> UpdateKeyAsync(T entity, string keyColumnName, string idName, object keyValue, object idValue, string? updatedBy="")
        {
            var tStampValue = entity.GetType().GetProperty("tStamp").GetValue(entity, null);

            var mapping = _dbContext.Model.FindEntityType(typeof(T));
            //var schema = mapping.GetSchema();
            var tableName = mapping.GetTableName();

            var tStamp = new SqlParameter("@tStamp", tStampValue) { Direction = System.Data.ParameterDirection.InputOutput };
            var result=0;

            if (string.IsNullOrEmpty(updatedBy)) {
                result = await _dbContext.Database.ExecuteSqlRawAsync(
                $"UPDATE [{tableName}] SET [{keyColumnName}] = @key WHERE [{idName}] = @id AND [tStamp] = @tStamp; SELECT @tStamp = [tStamp] FROM [{tableName}] WHERE @@ROWCOUNT = 1 AND [{idName}] = @id;",
                parameters: new object[]
                {
                    new SqlParameter("@key", keyValue),
                    new SqlParameter("@id", idValue),
                    tStamp
                });
            }
            else 
            {
                result = await _dbContext.Database.ExecuteSqlRawAsync(
                $"UPDATE [{tableName}] SET [{keyColumnName}] = @key,[UpdatedBy]=@updatedBy WHERE [{idName}] = @id AND [tStamp] = @tStamp; SELECT @tStamp = [tStamp] FROM [{tableName}] WHERE @@ROWCOUNT = 1 AND [{idName}] = @id;",
                parameters: new object[]
                {
                    new SqlParameter("@key", keyValue),
                    new SqlParameter("@id", idValue),
                    new SqlParameter("@updatedBy", updatedBy),
                    tStamp
                });
            }


            //Manually raise error DbUpdateConcurrencyException
            //It won't fire when using ExecuteSqlRawAsync
            if (result <= 0)
                throw new DbUpdateConcurrencyException("", new ReadOnlyCollection<IUpdateEntry>(new List<IUpdateEntry>() { null }));

            entity.GetType().GetProperty("tStamp").SetValue(entity, tStamp.Value);

            return result;
        }
    }
}
