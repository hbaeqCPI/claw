using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class DMSInventorService : DMSDisclosureChildService<DMSInventor>, IDMSInventorService
    {
        public DMSInventorService(
                ICPiDbContext cpiDbContext,
                IDisclosureService disclosureService,
                ClaimsPrincipal user
                ) : base(cpiDbContext, user, disclosureService)
        {
        }

        //UNFILTERED PAT INVENTORS FOR PICK LIST DATA
        public IQueryable<PatInventor> PatInventors => _cpiDbContext.GetRepository<PatInventor>().QueryableList;

        //this returns filtered inventors (those for disclosures accessible by user)
        public IQueryable<DMSInventor> QueryableListWithEntityFilter => QueryableList.Where(i => _disclosureService.QueryableList.Any(d => i.DMSId == d.DMSId));


        public override async Task<bool> Update(object key, string userName, IEnumerable<DMSInventor> updated, IEnumerable<DMSInventor> added, IEnumerable<DMSInventor> deleted)
        {
            int dmsId = (int)key;

            //must be inventors or modify users to update
            var cpiPermissions = CPiPermissions.Inventor;
            cpiPermissions.Add("modify");
            await _disclosureService.ValidatePermission(dmsId, cpiPermissions, true);

            //must be a default inventor or modify users to add/delete
            if (added.Any() || deleted.Any())
                Guard.Against.NoRecordPermission(await _disclosureService.IsUserDefaultInventor(dmsId) || await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null));

            var disclosure = await ValidateDisclosure(dmsId);
            disclosure.UpdatedBy = userName;
            disclosure.LastUpdate = DateTime.Now;           

            var inventorHistory = new List<DMSInventorHistory>();

            var oldInventorList = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSInventor>().QueryableList.Where(d => d.DMSId == dmsId).ToListAsync();

            foreach (var item in updated)
            {
                Guard.Against.Null(item.InventorID, "Inventor");
                item.UpdatedBy = userName;
                item.LastUpdate = disclosure.LastUpdate;

                if (item.OldInventorID != item.InventorID)
                {
                    inventorHistory.Add(new DMSInventorHistory()
                    {
                        DMSId = dmsId,
                        OldInventorID = item.OldInventorID,
                        NewInventorID = item.InventorID,
                        OldIsNonEmployee = oldInventorList.Where(d => d.DMSInventorID == item.DMSInventorID).Select(d => d.IsNonEmployee).FirstOrDefault(),
                        NewIsNonEmployee = item.IsNonEmployee,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = disclosure.LastUpdate,
                        LastUpdate = disclosure.LastUpdate
                    });
                }
            }

            foreach (var item in added)
            {
                Guard.Against.Null(item.InventorID, "Inventor");

                item.DMSId = dmsId;
                item.UpdatedBy = disclosure.UpdatedBy;
                item.LastUpdate = disclosure.LastUpdate;
                item.CreatedBy = disclosure.UpdatedBy;
                item.DateCreated = disclosure.LastUpdate;
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(dmsId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }

                inventorHistory.AddRange(added.Select(d => new DMSInventorHistory()
                {
                    DMSId = dmsId,                    
                    NewInventorID = d.InventorID,
                    NewIsNonEmployee = d.IsNonEmployee,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = disclosure.LastUpdate,
                    LastUpdate = disclosure.LastUpdate
                }));
            }

            if (deleted.Any())
            {
                inventorHistory.AddRange(deleted.Join(oldInventorList, del => del.DMSInventorID, old => old.DMSInventorID, (del, old) => new DMSInventorHistory()
                {
                    DMSId = dmsId,
                    OldInventorID = del.OldInventorID,
                    OldIsNonEmployee = old.IsNonEmployee,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = disclosure.LastUpdate,
                    LastUpdate = disclosure.LastUpdate
                }));
            }

            var repository = _cpiDbContext.GetRepository<DMSInventor>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<Disclosure>().Update(disclosure);

            var tempHistory = _cpiDbContext.GetRepository<DMSInventorHistory>().QueryableList.Where(d => d.DMSId == dmsId);
            _cpiDbContext.GetRepository<DMSInventorHistory>().Delete(tempHistory);

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.GetRepository<DMSInventorHistory>().Add(inventorHistory);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int dmsId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableListWithEntityFilter.Where(o => o.DMSId == dmsId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var thisInventor = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.DMSInventorID == id);
            Guard.Against.NoRecordPermission(thisInventor != null);
            thisInventor.UpdatedBy = userName;
            thisInventor.LastUpdate = DateTime.Now;

            int dmsId = thisInventor.DMSId;
            int oldIndex = thisInventor.OrderOfEntry;

            //must be inventor or modify users to update
            var cpiPermissions = CPiPermissions.Inventor;
            cpiPermissions.Add("modify");
            await _disclosureService.ValidatePermission(dmsId, cpiPermissions, true);

            //only default inventor or modify users can reorder
            Guard.Against.NoRecordPermission(await _disclosureService.IsUserDefaultInventor(dmsId) || await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null));

            var disclosure = await ValidateDisclosure(dmsId);
            disclosure.UpdatedBy = thisInventor.UpdatedBy;
            disclosure.LastUpdate = thisInventor.LastUpdate;


            List<DMSInventor> dmsInventors = new List<DMSInventor>();
            if (oldIndex > newIndex)
            {
                dmsInventors = await QueryableList.Where(w => w.DMSId == dmsId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                dmsInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                dmsInventors = await QueryableList.Where(w => w.DMSId == dmsId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                dmsInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            thisInventor.OrderOfEntry = newIndex;
            dmsInventors.Add(thisInventor);

            _cpiDbContext.GetRepository<DMSInventor>().Update(dmsInventors);
            _cpiDbContext.GetRepository<Disclosure>().Update(disclosure);
            await _cpiDbContext.SaveChangesAsync();
        }

        //TODO: IMPLEMENT
        public Task RefreshOrderOfEntry(int parentId)
        {
            throw new NotImplementedException();
        }
    }
}
