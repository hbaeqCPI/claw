using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class EntityContactService<T1, T2> : ChildEntityService<T1, T2>, IChildEntityService<T1, T2> where T1 : BaseEntity where T2 : BaseEntity
    {
        public EntityContactService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T2> updated, IEnumerable<T2> added, IEnumerable<T2> deleted)
        {
            var entityId = (int)key;

            if (EntityType.IsEntityFilterType(typeof(T1), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(entityId));

            List<LetterEntitySetting> orphanLetters = new List<LetterEntitySetting>();

            if (updated.Any())
                orphanLetters.AddRange(await GetOrphanLetterSettings(entityId, updated));

            if (deleted.Any())
                orphanLetters.AddRange(await GetOrphanLetterSettings(entityId, deleted));

            if (orphanLetters.Any())
                _cpiDbContext.GetRepository<LetterEntitySetting>().Delete(orphanLetters);

            //todo: enforce only one default

            return await base.Update(key, userName, updated, added, deleted);
        }

        private async Task<List<LetterEntitySetting>> GetOrphanLetterSettings(int entityId, IEnumerable<T2> contacts)
        {
            var entityType = EntityType.GetEntityType(typeof(T1));
            var contactIds = contacts.Where(c => ((int)c.GetType().GetProperty("GenAllLetters").GetValue(c, null)) != (int)LetterOption.Specific).Select(c => (int)c.GetType().GetProperty("ContactID").GetValue(c, null)).ToList();
            return await _cpiDbContext.GetRepository<LetterEntitySetting>().QueryableList.Where(l => l.EntityType == entityType && l.EntityId == entityId && contactIds.Any(c => c == l.ContactId)).ToListAsync();
        }
    }
}
