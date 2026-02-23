using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Clearance;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class TmcListService : TmcClearanceChildService<TmcList>, IMultipleEntityService<TmcClearance, TmcList>
    {
        public TmcListService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, ITmcClearanceService clearanceService) : base(cpiDbContext, user, clearanceService)
        {
        }

        public IQueryable<TmcList> QueryableListWithEntityFilter
        {
            get
            {
                return QueryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName,
            IEnumerable<TmcList> updated, IEnumerable<TmcList> added, IEnumerable<TmcList> deleted)
        {
            int tmcId = (int)key;
            var clearance = await ValidateClearance(tmcId);
            clearance.UpdatedBy = userName;
            clearance.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any() || deleted.Any())
                await _clearanceService.ValidatePermission(tmcId);

            //if (deleted.Any())
            //    await ValidateRole(CPiPermissions.FullModify);

            foreach (var item in updated)
            {
                item.UpdatedBy = clearance.UpdatedBy;
                item.LastUpdate = clearance.LastUpdate;
            }

            foreach (var item in added)
            {
                item.TmcId = tmcId;
                item.UpdatedBy = clearance.UpdatedBy;
                item.LastUpdate = clearance.LastUpdate;
                item.CreatedBy = clearance.UpdatedBy;
                item.DateCreated = clearance.LastUpdate;
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(tmcId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var repository = _cpiDbContext.GetRepository<TmcList>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<TmcClearance>().Update(clearance);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int tmcId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableList.Where(tmc => tmc.TmcId == tmcId).MaxAsync(tmc => tmc.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var clearanceList = await QueryableList.SingleOrDefaultAsync(tmc => tmc.ListId == id);
            Guard.Against.NoRecordPermission(clearanceList != null);
            clearanceList.UpdatedBy = userName;
            clearanceList.LastUpdate = DateTime.Now;

            int tmcId = clearanceList.TmcId;
            int oldIndex = clearanceList.OrderOfEntry;

            var clearance = await ValidateClearance(tmcId);
            clearance.UpdatedBy = clearanceList.UpdatedBy;
            clearance.LastUpdate = clearanceList.LastUpdate;

            //await ValidateRole(CPiPermissions.FullModify);            
            await _clearanceService.ValidatePermission(tmcId);

            List<TmcList> clearanceLists = new List<TmcList>();
            if (oldIndex > newIndex)
            {
                clearanceLists = await QueryableList.Where(w => w.TmcId == tmcId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                clearanceLists.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                clearanceLists = await QueryableList.Where(w => w.TmcId == tmcId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                clearanceLists.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            clearanceList.OrderOfEntry = newIndex;
            clearanceLists.Add(clearanceList);

            _cpiDbContext.GetRepository<TmcList>().Update(clearanceLists);
            _cpiDbContext.GetRepository<TmcClearance>().Update(clearance);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RefreshOrderOfEntry(int tmcId)
        {
            int sortOrder = 0;

            var clearanceLists = await QueryableList.Where(w => w.TmcId == tmcId).OrderBy(o => o.OrderOfEntry).ToListAsync();
            clearanceLists.ForEach(m => m.OrderOfEntry = sortOrder++);

            _cpiDbContext.GetRepository<TmcList>().Update(clearanceLists);
            await _cpiDbContext.SaveChangesAsync();
        }

    }
}
