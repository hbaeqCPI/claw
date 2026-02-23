using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class TmkOwnerService : TmkTrademarkChildService<TmkOwner>, IMultipleEntityService<TmkTrademark, TmkOwner>
    {
        private readonly ISystemSettings<TmkSetting> _settings;

        public TmkOwnerService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            ITmkTrademarkService trademarkService, 
            ISystemSettings<TmkSetting> settings) : base(cpiDbContext, user, trademarkService)
        {
            _settings = settings;
        }

        public IQueryable<TmkOwner> QueryableListWithEntityFilter
        {
            get
            {
                if (_trademarkService.IsOwnerRequired)
                    return QueryableList.Where(EntityFilter());

                return QueryableList;
            }
        }

        private Expression<Func<TmkOwner, bool>> EntityFilter()
        {
            return o => _cpiDbContext.GetEntityFilterRepository().UserEntityFilters.Any(ef => ef.UserId == _user.GetUserIdentifier() && ef.EntityId == o.OwnerID);
        }

        
        public override async Task<bool> Update(object key, string userName, IEnumerable<TmkOwner> updated, IEnumerable<TmkOwner> added, IEnumerable<TmkOwner> deleted)
        {
            int tmkId = (int)key;
            var trademark = await ValidateTrademark(tmkId);
            trademark.UpdatedBy = userName;
            trademark.LastUpdate = DateTime.Now;

            if (updated.Any() || added.Any())
                await ValidatePermission(CPiPermissions.FullModify, trademark.RespOffice);

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, trademark.RespOffice);

            var settings = await _settings.GetSetting();
            var ownerLabel = settings.LabelOwner;

            foreach (var item in deleted)
            {
                item.Owner = null;
                await ValidateEntityFilter(item.OwnerID);
            }

            if (_trademarkService.IsOwnerRequired && deleted.Any())
            {
                if (await QueryableListWithEntityFilter.CountAsync(o => o.TmkID == tmkId) <= deleted.Count())
                    Guard.Against.Null(null, ownerLabel);
            }

            foreach (var item in updated)
            {
                //Guard.Against.Null(item.Owner?.OwnerCode, ownerLabel);
                //item.OwnerID = item.Owner.OwnerID;
                item.Owner = null;
                item.UpdatedBy = trademark.UpdatedBy;
                item.LastUpdate = trademark.LastUpdate;
                await ValidateEntityFilter(item.OwnerID);
            }

            foreach (var item in added)
            {
                //Guard.Against.Null(item.Owner?.OwnerCode, ownerLabel);
                //item.OwnerID = item.Owner.OwnerID;
                item.TmkID = tmkId;
                item.Owner = null;
                item.UpdatedBy = trademark.UpdatedBy;
                item.LastUpdate = trademark.LastUpdate;
                item.CreatedBy = trademark.UpdatedBy;
                item.DateCreated = trademark.LastUpdate;
                await ValidateEntityFilter(item.OwnerID);
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(tmkId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var repository = _cpiDbContext.GetRepository<TmkOwner>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Add(added);

            _cpiDbContext.GetRepository<TmkTrademark>().Update(trademark);

            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int tmkId)
        {
            int? lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await QueryableList.Where(o => o.TmkID == tmkId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return (lastOrderOfEntry ?? 0) + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var tmkOwner = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.TmkOwnerID == id);
            Guard.Against.NoRecordPermission(tmkOwner != null);
            tmkOwner.UpdatedBy = userName;
            tmkOwner.LastUpdate = DateTime.Now;

            int tmkId = tmkOwner.TmkID;
            int oldIndex = tmkOwner.OrderOfEntry ?? 0;

            var trademark = await ValidateTrademark(tmkId);
            trademark.UpdatedBy = tmkOwner.UpdatedBy;
            trademark.LastUpdate = tmkOwner.LastUpdate;

            await ValidatePermission(CPiPermissions.FullModify, trademark.RespOffice);
            await ValidateEntityFilter(tmkOwner.OwnerID);

            List<TmkOwner> tmkOwners = new List<TmkOwner>();
            if (oldIndex > newIndex)
            {
                tmkOwners = await QueryableList.Where(w => w.TmkID == tmkId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                tmkOwners.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                tmkOwners = await QueryableList.Where(w => w.TmkID == tmkId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                tmkOwners.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            tmkOwner.OrderOfEntry = newIndex;
            tmkOwners.Add(tmkOwner);

            _cpiDbContext.GetRepository<TmkOwner>().Update(tmkOwners);
            _cpiDbContext.GetRepository<TmkTrademark>().Update(trademark);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RefreshOrderOfEntry(int parentId)
        {
            int sortOrder = 0;

            var tmkOwners = await QueryableList.Where(w => w.TmkID == parentId).OrderBy(o => o.OrderOfEntry).ToListAsync();
            tmkOwners.ForEach(m => m.OrderOfEntry = sortOrder++);

            _cpiDbContext.GetRepository<TmkOwner>().Update(tmkOwners);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task ValidateEntityFilter(int entityId)
        {
            if (_trademarkService.IsOwnerRequired || _user.GetEntityFilterType() == Identity.CPiEntityType.Owner)
            {
                var settings = await _settings.GetSetting();
                Guard.Against.NoFieldPermission(await EntityFilterAllowed(entityId), settings.LabelOwner);
            }
        }
    }
}
