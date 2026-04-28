using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Entities;
using LawPortal.Core.Exceptions;
using LawPortal.Core.Helpers;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Services
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        protected readonly ICPiDbContext _cpiDbContext;

        public BaseService(ICPiDbContext cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
        }

        public virtual IQueryable<T> QueryableList => _cpiDbContext.GetRepository<T>().QueryableList;

        public virtual async Task<T> GetByIdAsync(int entityId)
        {
            return await _cpiDbContext.GetRepository<T>().GetByIdAsync(entityId);
        }

        public virtual async Task Add(T entity)
        {
            _cpiDbContext.GetRepository<T>().Add(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task Update(T entity)
        {
            _cpiDbContext.GetRepository<T>().Update(entity);
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(entity);
        }

        public virtual async Task Delete(T entity)
        {
            _cpiDbContext.GetRepository<T>().Delete(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task Add(IEnumerable<T> entities)
        {
            _cpiDbContext.GetRepository<T>().Add(entities);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task Update(IEnumerable<T> entities)
        {
            _cpiDbContext.GetRepository<T>().Update(entities);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task Delete(IEnumerable<T> entities)
        {
            _cpiDbContext.GetRepository<T>().Delete(entities);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task UpdateRemarks(T entity)
        {
            var entityType = entity.GetType();
            if (entityType.GetProperty("Remarks") != null)
            {
                var id = (int)entityType.GetProperty(_cpiDbContext.GetRepository<T>().PrimaryKey.Name).GetValue(entity, null);
                var updated = await GetByIdAsync(id);

                Guard.Against.NoRecordPermission(updated != null);

                entityType.GetProperty("tStamp").SetValue(updated, entityType.GetProperty("tStamp").GetValue(entity, null));

                _cpiDbContext.GetRepository<T>().Attach(updated);
                entityType.GetProperty("Remarks").SetValue(updated, entityType.GetProperty("Remarks").GetValue(entity, null));
                entityType.GetProperty("UpdatedBy").SetValue(updated, entityType.GetProperty("UpdatedBy").GetValue(entity, null));
                entityType.GetProperty("LastUpdate").SetValue(updated, entityType.GetProperty("LastUpdate").GetValue(entity, null));

                await _cpiDbContext.SaveChangesAsync();
            }
        }
    }
}
