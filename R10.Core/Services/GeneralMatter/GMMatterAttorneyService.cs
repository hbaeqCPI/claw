using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.GeneralMatter;
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

namespace R10.Core.Services.GeneralMatter
{
    public class GMMatterAttorneyService : GMMatterChildService<GMMatterAttorney>, IGMMatterAttorneyService
    {
        public GMMatterAttorneyService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IGMMatterService matterService) : base(cpiDbContext, user, matterService)
        {
        }

        public IQueryable<GMMatterAttorney> QueryableListWithEntityFilter
        {
            get
            {
                if (_matterService.IsAttorneyRequired)
                    return QueryableList.Where(EntityFilter());

                return QueryableList;
            }
        }

        private Expression<Func<GMMatterAttorney, bool>> EntityFilter()
        {
            return a => _cpiDbContext.GetEntityFilterRepository().UserEntityFilters.Any(ef => ef.UserId == _user.GetUserIdentifier() && ef.EntityId == a.AttorneyID);
        }

        public override async Task<bool> Update(object key, string userName,
            IEnumerable<GMMatterAttorney> updated, IEnumerable<GMMatterAttorney> added, IEnumerable<GMMatterAttorney> deleted)
        {
            return await Update(key, userName, updated, added, deleted, CPiPermissions.FullModify, CPiPermissions.CanDelete);
        }

        public async Task<bool> Update(object key, string userName,
            IEnumerable<GMMatterAttorney> updated, IEnumerable<GMMatterAttorney> added, IEnumerable<GMMatterAttorney> deleted,
            List<string> canModifyRoles, List<string> canDeleteRoles)
        {
            int matId = (int)key;
            var matter = await ValidateMatter(matId);
            matter.UpdatedBy = userName;
            matter.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any())
                await ValidatePermission(canModifyRoles, matter.RespOffice);

            if (deleted.Any())
                await ValidatePermission(canDeleteRoles, matter.RespOffice);

            foreach (var item in deleted)
            {
                item.Attorney = null;
                await ValidateEntityFilter(item.AttorneyID);
            }

            if (_matterService.IsAttorneyRequired && deleted.Any())
            {
                if (await QueryableListWithEntityFilter.CountAsync(a => a.MatId == matId) <= deleted.Count())
                    Guard.Against.Null(null, "Attorney");
            }

            foreach (var item in updated)
            {
                Guard.Against.Null(item.Attorney?.AttorneyCode, "Attorney");

                item.AttorneyID = item.Attorney.AttorneyID;
                item.Attorney = null;
                item.UpdatedBy = matter.UpdatedBy;
                item.LastUpdate = matter.LastUpdate;
                await ValidateEntityFilter(item.AttorneyID);
            }

            foreach (var item in added)
            {
                Guard.Against.Null(item.Attorney?.AttorneyCode, "Attorney");

                item.MatId = matId;
                item.AttorneyID = item.Attorney.AttorneyID;
                item.Attorney = null;
                item.UpdatedBy = matter.UpdatedBy;
                item.LastUpdate = matter.LastUpdate;
                item.CreatedBy = matter.UpdatedBy;
                item.DateCreated = matter.LastUpdate;
                await ValidateEntityFilter(item.AttorneyID);
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(matId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var repository = _cpiDbContext.GetRepository<GMMatterAttorney>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<GMMatter>().Update(matter);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int matId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableList.Where(ma => ma.MatId == matId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var matterAttorney = await QueryableListWithEntityFilter.SingleOrDefaultAsync(ma => ma.AttID == id);
            Guard.Against.NoRecordPermission(matterAttorney != null);
            matterAttorney.UpdatedBy = userName;
            matterAttorney.LastUpdate = DateTime.Now;

            int matId = matterAttorney.MatId;
            int oldIndex = matterAttorney.OrderOfEntry;

            var matter = await ValidateMatter(matId);
            matter.UpdatedBy = matterAttorney.UpdatedBy;
            matter.LastUpdate = matterAttorney.LastUpdate;

            await ValidatePermission(CPiPermissions.FullModify, matter.RespOffice);
            await ValidateEntityFilter(matterAttorney.AttorneyID);

            List<GMMatterAttorney> matterAttorneys = new List<GMMatterAttorney>();
            if (oldIndex > newIndex)
            {
                matterAttorneys = await QueryableList.Where(w => w.MatId == matId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                matterAttorneys.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                matterAttorneys = await QueryableList.Where(w => w.MatId == matId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                matterAttorneys.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            matterAttorney.OrderOfEntry = newIndex;
            matterAttorneys.Add(matterAttorney);

            _cpiDbContext.GetRepository<GMMatterAttorney>().Update(matterAttorneys);
            _cpiDbContext.GetRepository<GMMatter>().Update(matter);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RefreshOrderOfEntry(int matId)
        {
            int sortOrder = 0;

            var matterAttorneys = await QueryableList.Where(w => w.MatId == matId).OrderBy(o => o.OrderOfEntry).ToListAsync();
            matterAttorneys.ForEach(m => m.OrderOfEntry = sortOrder++);

            _cpiDbContext.GetRepository<GMMatterAttorney>().Update(matterAttorneys);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task ValidateEntityFilter(int entityId)
        {
            if (_matterService.IsAttorneyRequired)
            {
                Guard.Against.NoFieldPermission(await EntityFilterAllowed(entityId), "Attorney");
            }
        }
    }
}
