using Microsoft.EntityFrameworkCore;
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
    public class PatInventorInvService : PatInventionChildService<PatInventorInv>, IMultipleEntityService<Invention, PatInventorInv>
    {
        public PatInventorInvService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IInventionService inventionService) : base(cpiDbContext, user, inventionService)
        {
        }

        public IQueryable<PatInventorInv> QueryableListWithEntityFilter
        {
            get
            {
                if (_inventionService.IsInventorRequired)
                    return QueryableList.Where(EntityFilter());

                return QueryableList;
            }
        }

        private Expression<Func<PatInventorInv, bool>> EntityFilter()
        {
            return i => _cpiDbContext.GetEntityFilterRepository().UserEntityFilters.Any(ef => ef.UserId == _user.GetUserIdentifier() && ef.EntityId == i.InventorID);
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<PatInventorInv> updated, IEnumerable<PatInventorInv> added, IEnumerable<PatInventorInv> deleted)
        {
            int invId = (int)key;
            var invention = await ValidateInvention(invId);
            invention.UpdatedBy = userName;
            invention.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any())
                await ValidatePermission(CPiPermissions.FullModify, invention.RespOffice);

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, invention.RespOffice);

            foreach (var item in deleted)
            {
                item.InventorInvInventor = null;
                await ValidateEntityFilter(item.InventorID);
            }

            if (_inventionService.IsInventorRequired && deleted.Any())
            {
                if (await QueryableListWithEntityFilter.CountAsync(o => o.InvId == invId) <= deleted.Count())
                    Guard.Against.Null(null, "Inventor");
            }

            foreach (var item in updated)
            {
                Guard.Against.Null(item.InventorInvInventor?.Inventor, "Inventor");

                item.InventorID = item.InventorInvInventor.InventorID;
                item.InventorInvInventor = null;
                item.UpdatedBy = invention.UpdatedBy;
                item.LastUpdate = invention.LastUpdate;
                await ValidateEntityFilter(item.InventorID);
            }

            foreach (var item in added)
            {
                Guard.Against.Null(item.InventorInvInventor?.Inventor, "Inventor");

                item.InvId = invId;
                item.InventorID = item.InventorInvInventor.InventorID;
                item.InventorInvInventor = null;
                item.UpdatedBy = invention.UpdatedBy;
                item.LastUpdate = invention.LastUpdate;
                item.CreatedBy = invention.UpdatedBy;
                item.DateCreated = invention.LastUpdate;
                await ValidateEntityFilter(item.InventorID);
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(invId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var repository = _cpiDbContext.GetRepository<PatInventorInv>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<Invention>().Update(invention);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int invId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableList.Where(o => o.InvId == invId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var invInventor = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.InventorInvID == id);
            Guard.Against.NoRecordPermission(invInventor != null);
            invInventor.UpdatedBy = userName;
            invInventor.LastUpdate = DateTime.Now;

            int invId = invInventor.InvId;
            int oldIndex = invInventor.OrderOfEntry;

            var invention = await ValidateInvention(invId);
            invention.UpdatedBy = invInventor.UpdatedBy;
            invention.LastUpdate = invInventor.LastUpdate;

            await ValidatePermission(CPiPermissions.FullModify, invention.RespOffice);
            await ValidateEntityFilter(invInventor.InventorID);

            List<PatInventorInv> invInventors = new List<PatInventorInv>();
            if (oldIndex > newIndex)
            {
                invInventors = await QueryableList.Where(w => w.InvId == invId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                invInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                invInventors = await QueryableList.Where(w => w.InvId == invId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                invInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            invInventor.OrderOfEntry = newIndex;
            invInventors.Add(invInventor);

            _cpiDbContext.GetRepository<PatInventorInv>().Update(invInventors);
            _cpiDbContext.GetRepository<Invention>().Update(invention);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RefreshOrderOfEntry(int invId)
        {
            int sortOrder = 0;

            var invInventors = await QueryableList.Where(w => w.InvId == invId).OrderBy(o => o.OrderOfEntry).ToListAsync();
            invInventors.ForEach(m => m.OrderOfEntry = sortOrder++);

            _cpiDbContext.GetRepository<PatInventorInv>().Update(invInventors);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task ValidateEntityFilter(int entityId)
        {
            if (_inventionService.IsInventorRequired)
            {
                Guard.Against.NoFieldPermission(await EntityFilterAllowed(entityId), "Inventor");
            }
        }
    }
}
