using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PacInventorService : PacClearanceChildService<PacInventor>, IPacInventorService
    {
        public PacInventorService(
                ICPiDbContext cpiDbContext,
                IPacClearanceService clearanceService,
                ClaimsPrincipal user
                ) : base(cpiDbContext, user, clearanceService)
        {
        }

        //UNFILTERED PAT INVENTORS FOR PICK LIST DATA
        public IQueryable<PatInventor> PatInventors => _cpiDbContext.GetRepository<PatInventor>().QueryableList;

        //this returns filtered inventors (those for pat clearances accessible by user)
        public IQueryable<PacInventor> QueryableListWithEntityFilter => QueryableList.Where(i => _clearanceService.QueryableList.Any(d => i.PacId == d.PacId));


        public override async Task<bool> Update(object key, string userName, IEnumerable<PacInventor> updated, IEnumerable<PacInventor> added, IEnumerable<PacInventor> deleted)
        {
            int pacId = (int)key;            
            var clearance = await ValidateClearance(pacId);
            clearance.UpdatedBy = userName;
            clearance.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any() || deleted.Any())
                await _clearanceService.ValidatePermission(pacId);

            foreach (var item in updated)
            {
                Guard.Against.Null(item.InventorID, "Inventor");
                item.UpdatedBy = userName;
                item.LastUpdate = clearance.LastUpdate;
            }

            foreach (var item in added)
            {
                Guard.Against.Null(item.InventorID, "Inventor");

                item.PacId = pacId;
                item.UpdatedBy = clearance.UpdatedBy;
                item.LastUpdate = clearance.LastUpdate;
                item.CreatedBy = clearance.UpdatedBy;
                item.DateCreated = clearance.LastUpdate;
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(pacId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var repository = _cpiDbContext.GetRepository<PacInventor>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<PacClearance>().Update(clearance);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int pacId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableListWithEntityFilter.Where(o => o.PacId == pacId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var thisInventor = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.PacInventorID == id);
            Guard.Against.NoRecordPermission(thisInventor != null);
            thisInventor.UpdatedBy = userName;
            thisInventor.LastUpdate = DateTime.Now;

            int pacId = thisInventor.PacId;
            int oldIndex = thisInventor.OrderOfEntry;

            //only modify users can reorder
            //Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.PatClearance, CPiPermissions.FullModify, ""));

            var clearance = await ValidateClearance(pacId);
            clearance.UpdatedBy = thisInventor.UpdatedBy;
            clearance.LastUpdate = thisInventor.LastUpdate;


            List<PacInventor> pacInventors = new List<PacInventor>();
            if (oldIndex > newIndex)
            {
                pacInventors = await QueryableList.Where(w => w.PacId == pacId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                pacInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                pacInventors = await QueryableList.Where(w => w.PacId == pacId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                pacInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            thisInventor.OrderOfEntry = newIndex;
            pacInventors.Add(thisInventor);

            _cpiDbContext.GetRepository<PacInventor>().Update(pacInventors);
            _cpiDbContext.GetRepository<PacClearance>().Update(clearance);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RefreshOrderOfEntry(int parentId)
        {
            int sortOrder = 0;

            var clearanceInventors = await QueryableList.Where(w => w.PacId == parentId).OrderBy(o => o.OrderOfEntry).ToListAsync();
            clearanceInventors.ForEach(m => m.OrderOfEntry = sortOrder++);

            _cpiDbContext.GetRepository<PacInventor>().Update(clearanceInventors);
            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
