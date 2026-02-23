using R10.Core.Entities;
using R10.Core.Entities.Patent;
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
    public class ClientDesignatedCountryService : ChildEntityService<Client, ClientDesignatedCountry>, IClientDesignatedCountryService
    {
        public ClientDesignatedCountryService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public List<string> ValidSystems => new List<string>() { SystemType.Patent, SystemType.Trademark };

        public override async Task<bool> Update(object key, string userName, IEnumerable<ClientDesignatedCountry> updated, IEnumerable<ClientDesignatedCountry> added, IEnumerable<ClientDesignatedCountry> deleted)
        {
            var entityId = (int)key;

            if(_user.GetEntityFilterType() == CPiEntityType.Client)
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(entityId));

            //todo: cascade delete
            //todo: delete child des ctrys when parent country is updated
            return await base.Update(key, userName, updated, added, deleted);
        }

        public async Task<bool> Update(object key, int parentId, string systemType, string userName, IEnumerable<ClientDesignatedCountry> updated, IEnumerable<ClientDesignatedCountry> added, IEnumerable<ClientDesignatedCountry> deleted)
        {
            var entityId = (int)key;

            if (_user.GetEntityFilterType() == CPiEntityType.Client)
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(entityId));

            var parent = await _cpiDbContext.GetRepository<Client>().GetByIdAsync(key);
            _cpiDbContext.GetRepository<Client>().Attach(parent);
            parent.UpdatedBy = userName;
            parent.LastUpdate = DateTime.Now;

            foreach (var item in updated)
            {
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            foreach (var item in added)
            {
                item.ClientID = entityId;
                item.ParentDesCtryID = parentId;
                item.SystemType = systemType;
                item.CreatedBy = parent.UpdatedBy;
                item.DateCreated = parent.LastUpdate;
                item.UpdatedBy = parent.UpdatedBy;
                item.LastUpdate = parent.LastUpdate;
            }

            var repository = _cpiDbContext.GetRepository<ClientDesignatedCountry>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }
    }
}
