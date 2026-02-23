using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PatOwnerInvService : PatInventionChildService<PatOwnerInv>, IMultipleEntityService<Invention, PatOwnerInv>
    {
        private readonly ISystemSettings<PatSetting> _settings;

        public PatOwnerInvService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            IInventionService inventionService, 
            ISystemSettings<PatSetting> settings) : base(cpiDbContext, user, inventionService)
        {
            _settings = settings;
        }

        public IQueryable<PatOwnerInv> QueryableListWithEntityFilter
        {
            get
            {
                if (_inventionService.IsOwnerRequired)
                    return QueryableList.Where(EntityFilter());

                return QueryableList;
            }
        }

        private Expression<Func<PatOwnerInv, bool>> EntityFilter()
        {
            return o => _cpiDbContext.GetEntityFilterRepository().UserEntityFilters.Any(ef => ef.UserId == _user.GetUserIdentifier() && ef.EntityId == o.OwnerID);
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<PatOwnerInv> updated, IEnumerable<PatOwnerInv> added, IEnumerable<PatOwnerInv> deleted)
        {
            int invId = (int)key;
            var invention = await ValidateInvention(invId);
            invention.UpdatedBy = userName;
            invention.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any())
                await ValidatePermission(CPiPermissions.FullModify, invention.RespOffice);

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, invention.RespOffice);

            var settings = await _settings.GetSetting();
            var ownerLabel = settings.LabelOwner;

            foreach (var item in deleted)
            {
                item.Owner = null;
                await ValidateEntityFilter(item.OwnerID);
            }

            if (_inventionService.IsOwnerRequired && deleted.Any())
            {
                if (await QueryableListWithEntityFilter.CountAsync(o => o.InvId == invId) <= deleted.Count())
                    Guard.Against.Null(null, ownerLabel);
            }

            foreach (var item in updated)
            {
                //Guard.Against.Null(item.Owner?.OwnerCode, ownerLabel);
                //item.OwnerID = item.Owner.OwnerID;
                item.Owner = null;
                item.UpdatedBy = invention.UpdatedBy;
                item.LastUpdate = invention.LastUpdate;
                await ValidateEntityFilter(item.OwnerID);
            }

            foreach (var item in added)
            {
                //Guard.Against.Null(item.Owner?.OwnerCode, ownerLabel);
                //item.OwnerID = item.Owner.OwnerID;
                item.InvId = invId;
                item.Owner = null;
                item.UpdatedBy = invention.UpdatedBy;
                item.LastUpdate = invention.LastUpdate;
                item.CreatedBy = invention.UpdatedBy;
                item.DateCreated = invention.LastUpdate;
                await ValidateEntityFilter(item.OwnerID);
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(invId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var repository = _cpiDbContext.GetRepository<PatOwnerInv>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<Invention>().Update(invention);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int invId)
        {
            int? lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableList.Where(o => o.InvId == invId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return (lastOrderOfEntry ?? 0) + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var invOwner = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.OwnerInvID == id);
            Guard.Against.NoRecordPermission(invOwner != null);
            invOwner.UpdatedBy = userName;
            invOwner.LastUpdate = DateTime.Now;

            int invId = invOwner.InvId;
            int oldIndex = invOwner.OrderOfEntry ?? 0;

            var invention = await ValidateInvention(invId);
            invention.UpdatedBy = invOwner.UpdatedBy;
            invention.LastUpdate = invOwner.LastUpdate;

            await ValidatePermission(CPiPermissions.FullModify, invention.RespOffice);
            await ValidateEntityFilter(invOwner.OwnerID);

            List<PatOwnerInv> invOwners = new List<PatOwnerInv>();
            if (oldIndex > newIndex)
            {
                invOwners = await QueryableList.Where(w => w.InvId == invId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                invOwners.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                invOwners = await QueryableList.Where(w => w.InvId == invId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                invOwners.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            invOwner.OrderOfEntry = newIndex;
            invOwners.Add(invOwner);

            _cpiDbContext.GetRepository<PatOwnerInv>().Update(invOwners);
            _cpiDbContext.GetRepository<Invention>().Update(invention);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RefreshOrderOfEntry(int parentId)
        {
            int sortOrder = 0;

            var invOwners = await QueryableList.Where(w => w.InvId == parentId).OrderBy(o => o.OrderOfEntry).ToListAsync();
            invOwners.ForEach(m => m.OrderOfEntry = sortOrder++);

            _cpiDbContext.GetRepository<PatOwnerInv>().Update(invOwners);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task ValidateEntityFilter(int entityId)
        {
            if (_inventionService.IsOwnerRequired || _user.GetEntityFilterType() == Identity.CPiEntityType.Owner)
            {
                var settings = await _settings.GetSetting();
                Guard.Against.NoFieldPermission(await EntityFilterAllowed(entityId), settings.LabelOwner);
            }
        }
    }
}
