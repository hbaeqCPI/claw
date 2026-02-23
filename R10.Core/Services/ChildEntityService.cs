using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class ChildEntityService<T1, T2> : EntityService<T2>, IChildEntityService<T1, T2> where T1 : BaseEntity where T2 : BaseEntity
    {
        public ChildEntityService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public virtual async Task<bool> Update(object key, string userName, IEnumerable<T2> updated, IEnumerable<T2> added, IEnumerable<T2> deleted)
        {
            T1 parent = await _cpiDbContext.GetRepository<T1>().GetByIdAsync(key);

            Guard.Against.NoRecordPermission(parent != null);

            _cpiDbContext.GetRepository<T1>().Attach(parent);
            parent.UpdatedBy = userName;
            parent.LastUpdate = DateTime.Now;

            foreach (var item in updated)
            {
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            foreach (var item in added)
            {
                item.GetType().GetProperty(_cpiDbContext.GetRepository<T1>().PrimaryKey.Name).SetValue(item, key);
                item.CreatedBy = parent.UpdatedBy;
                item.DateCreated = parent.LastUpdate;
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<T2>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }
    }
}
