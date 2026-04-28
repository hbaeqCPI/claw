using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Exceptions;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Services
{
    public class AuxService<T>  : EntityService<T>, IEntityService<T> where T : class
    {
        public AuxService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base (cpiDbContext, user)
        {
        }

        public override async Task<T> GetByIdAsync(int entityId)
        {
            return await _cpiDbContext.GetRepository<T>().GetByKeyAsync(entityId);
        }

        public override async Task Update(T entity)
        {
            await _cpiDbContext.GetRepository<T>().UpdateAsync(entity);
            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task UpdateRemarks(T entity)
        {
            var entityType = entity.GetType();
            if (entityType.GetProperty("Remarks") != null)
            {
                var id = (int)entityType.GetProperty(_cpiDbContext.GetRepository<T>().SurrogateKey.Name).GetValue(entity, null);
                var updated = await GetByIdAsync(id);

                Guard.Against.NoRecordPermission(updated != null);

                _cpiDbContext.GetRepository<T>().Attach(updated);

                entityType.GetProperty("Remarks").SetValue(updated, entityType.GetProperty("Remarks").GetValue(entity, null));
                entityType.GetProperty("UpdatedBy").SetValue(updated, entityType.GetProperty("UpdatedBy").GetValue(entity, null));
                entityType.GetProperty("LastUpdate").SetValue(updated, entityType.GetProperty("LastUpdate").GetValue(entity, null));
                entityType.GetProperty("tStamp").SetValue(updated, entityType.GetProperty("tStamp").GetValue(entity, null));

                await _cpiDbContext.SaveChangesAsync();
            }
        }
    }
}
