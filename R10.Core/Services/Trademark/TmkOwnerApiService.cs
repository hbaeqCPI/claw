using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
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

namespace R10.Core.Services.Trademark
{
    public interface ITmkOwnerApiService : IWebApiBaseService<TmkOwnerWebSvc, TmkOwner>
    {
        Task Delete(List<TmkOwnerWebSvc> owners, DateTime runDate);
    }

    public class TmkOwnerApiService : WebApiBaseService<TmkOwnerWebSvc>, ITmkOwnerApiService
    {
        private readonly IMultipleEntityService<TmkTrademark, TmkOwner> _tmkOwnerService;

        public TmkOwnerApiService(
            IMultipleEntityService<TmkTrademark, TmkOwner> tmkOwnerService,
            ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _tmkOwnerService = tmkOwnerService;
        }

        IQueryable<TmkOwner> IWebApiBaseService<TmkOwnerWebSvc, TmkOwner>.QueryableList => _tmkOwnerService.QueryableListWithEntityFilter;

        public Task<int> Add(TmkOwnerWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public Task Update(int id, TmkOwnerWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<int>> Import(List<TmkOwnerWebSvc> owners, DateTime runDate)
        {
            var tmkId = owners.Select(o => o.TmkId).FirstOrDefault();
            Guard.Against.NullOrZero(tmkId, "TmkId");

            var added = new List<TmkOwner>();
            var errors = new List<string>();

            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                try
                {
                    Guard.Against.NullOrZero(owner.OwnerID, "OwnerID");
                    Guard.Against.NoRecordPermission(await ValidateEntityFilter(owner.OwnerID ?? 0));

                    added.Add(new TmkOwner()
                    {
                        TmkID = tmkId,
                        OwnerID = owner.OwnerID ?? 0,
                        Percentage = owner.Percentage,
                        DateCreated = runDate,
                        CreatedBy = _user.GetUserName(),
                        LastUpdate = runDate,
                        UpdatedBy = _user.GetUserName()
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, owner.TmkId.ToString(), owner.TmkOwnerId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _tmkOwnerService.Update(tmkId, _user.GetUserName(), new List<TmkOwner>(), added, new List<TmkOwner>());

            //get new TmkOwnerIds
            foreach (var owner in owners)
            {
                var appOwner = added.FirstOrDefault(o => o.TmkID == owner.TmkId && o.OwnerID == owner.OwnerID);
                if (appOwner != null)
                    owner.TmkOwnerId = appOwner.TmkOwnerID;
            }

            return added.Select(o => o.TmkOwnerID).ToList();
        }

        public async Task Update(List<TmkOwnerWebSvc> owners, DateTime runDate)
        {
            var tmkId = owners.Select(o => o.TmkId).FirstOrDefault();
            Guard.Against.NullOrZero(tmkId, "TmkId");

            var tmkOwnerIds = owners.Select(o => o.TmkOwnerId).ToList();
            var tmkOwners = await _tmkOwnerService.QueryableListWithEntityFilter.Where(u => u.TmkID == tmkId && tmkOwnerIds.Contains(u.TmkOwnerID)).ToListAsync();
            var updated = new List<TmkOwner>();
            var errors = new List<string>();

            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                var tmkOwner = tmkOwners.FirstOrDefault(o => o.TmkOwnerID == owner.TmkOwnerId);
                try
                {
                    Guard.Against.RecordNotFound(tmkOwner != null);

                    if (tmkOwner != null)
                    {
                        Guard.Against.NoRecordPermission(await ValidateEntityFilter(tmkOwner.OwnerID));

                        if (owner.OwnerID != null)
                        {
                            Guard.Against.NoRecordPermission(await ValidateEntityFilter(owner.OwnerID ?? 0));
                            tmkOwner.OwnerID = owner.OwnerID ?? 0;
                        }

                        if (tmkOwner.Percentage != null)
                            tmkOwner.Percentage = owner.Percentage;

                        tmkOwner.LastUpdate = runDate;
                        tmkOwner.UpdatedBy = _user.GetUserName();

                        updated.Add(tmkOwner);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, owner.TmkId.ToString(), owner.TmkOwnerId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _tmkOwnerService.Update(tmkId, _user.GetUserName(), updated, new List<TmkOwner>(), new List<TmkOwner>());
        }

        public async Task Delete(List<TmkOwnerWebSvc> owners, DateTime runDate)
        {
            var tmkId = owners.Select(o => o.TmkId).FirstOrDefault();
            Guard.Against.NullOrZero(tmkId, "TmkId");

            var tmkOwnerIds = owners.Select(o => o.TmkOwnerId).ToList();
            var tmkOwners = await _tmkOwnerService.QueryableListWithEntityFilter.Where(u => u.TmkID == tmkId && tmkOwnerIds.Contains(u.TmkOwnerID)).ToListAsync();
            var deleted = new List<TmkOwner>();
            var errors = new List<string>();

            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                var tmkOwner = tmkOwners.FirstOrDefault(o => o.TmkOwnerID == owner.TmkOwnerId);

                try
                {
                    Guard.Against.RecordNotFound(tmkOwner != null);

                    if (tmkOwner != null)
                    {
                        Guard.Against.NoRecordPermission(await ValidateEntityFilter(tmkOwner.OwnerID));

                        owner.OwnerID = tmkOwner.OwnerID;
                        owner.Percentage = tmkOwner.Percentage;
                        deleted.Add(tmkOwner);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, owner.TmkId.ToString(), owner.TmkOwnerId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _tmkOwnerService.Update(tmkId, _user.GetUserName(), new List<TmkOwner>(), new List<TmkOwner>(), deleted);
        }

        private async Task<bool> ValidateEntityFilter(int entityId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Owner)
                return await _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList.AnyAsync(f => f.UserId == _user.GetUserIdentifier() && f.EntityId == entityId);
            else
                return true;
        }
    }
}
