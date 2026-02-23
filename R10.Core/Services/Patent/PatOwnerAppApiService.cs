using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
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
using System.Runtime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace R10.Core.Services.Patent
{
    public interface IPatOwnerAppApiService : IWebApiBaseService<PatOwnerAppWebSvc, PatOwnerApp>
    {
        Task Delete(List<PatOwnerAppWebSvc> owners, DateTime runDate);
    }

    public class PatOwnerAppApiService : WebApiBaseService<PatOwnerAppWebSvc>, IPatOwnerAppApiService
    {
        private readonly IMultipleEntityService<PatOwnerApp> _patOwnerAppService;

        public PatOwnerAppApiService(
            IMultipleEntityService<PatOwnerApp> patOwnerAppService,
            ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _patOwnerAppService = patOwnerAppService;
        }

        IQueryable<PatOwnerApp> IWebApiBaseService<PatOwnerAppWebSvc, PatOwnerApp>.QueryableList => _patOwnerAppService.QueryableListWithEntityFilter;

        public Task<int> Add(PatOwnerAppWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public Task Update(int id, PatOwnerAppWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<int>> Import(List<PatOwnerAppWebSvc> owners, DateTime runDate)
        {
            var appId = owners.Select(o => o.AppId).FirstOrDefault();
            Guard.Against.NullOrZero(appId, "AppId");

            var added = new List<PatOwnerApp>();
            var errors = new List<string>();

            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                try
                {
                    Guard.Against.NullOrZero(owner.OwnerID, "OwnerID");
                    Guard.Against.NoRecordPermission(await ValidateEntityFilter(owner.OwnerID ?? 0));

                    added.Add(new PatOwnerApp()
                    {
                        AppId = appId,
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
                    errors.Add(FormatErrorMessage(i, ex.Message, owner.AppId.ToString(), owner.OwnerAppId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _patOwnerAppService.Update(appId, _user.GetUserName(), new List<PatOwnerApp>(), added, new List<PatOwnerApp>());

            //get new OwnerAppIds
            foreach (var owner in owners)
            {
                var appOwner = added.FirstOrDefault(o => o.AppId == owner.AppId && o.OwnerID == owner.OwnerID);
                if (appOwner != null)
                    owner.OwnerAppId = appOwner.OwnerAppID;
            }

            return added.Select(o => o.OwnerAppID).ToList();
        }

        public async Task Update(List<PatOwnerAppWebSvc> owners, DateTime runDate)
        {
            var appId = owners.Select(o => o.AppId).FirstOrDefault();
            Guard.Against.NullOrZero(appId, "AppId");

            var ownerAppIds = owners.Select(o => o.OwnerAppId).ToList();
            var appOwners = await _patOwnerAppService.QueryableListWithEntityFilter.Where(u => u.AppId == appId && ownerAppIds.Contains(u.OwnerAppID)).ToListAsync();
            var updated = new List<PatOwnerApp>();
            var errors = new List<string>();

            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                var appOwner = appOwners.FirstOrDefault(o => o.OwnerAppID == owner.OwnerAppId);
                try
                {
                    Guard.Against.RecordNotFound(appOwner != null);

                    if (appOwner != null)
                    {
                        Guard.Against.NoRecordPermission(await ValidateEntityFilter(appOwner.OwnerID));

                        if (owner.OwnerID != null)
                        {
                            Guard.Against.NoRecordPermission(await ValidateEntityFilter(owner.OwnerID ?? 0));
                            appOwner.OwnerID = owner.OwnerID ?? 0;
                        }

                        if (appOwner.Percentage != null)
                            appOwner.Percentage = owner.Percentage;

                        appOwner.LastUpdate = runDate;
                        appOwner.UpdatedBy = _user.GetUserName();

                        updated.Add(appOwner);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, owner.AppId.ToString(), owner.OwnerAppId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _patOwnerAppService.Update(appId, _user.GetUserName(), updated, new List<PatOwnerApp>(), new List<PatOwnerApp>());
        }

        public async Task Delete(List<PatOwnerAppWebSvc> owners, DateTime runDate)
        {
            var appId = owners.Select(o => o.AppId).FirstOrDefault();
            Guard.Against.NullOrZero(appId, "AppId");

            var ownerAppIds = owners.Select(o => o.OwnerAppId).ToList();
            var appOwners = await _patOwnerAppService.QueryableListWithEntityFilter.Where(u => u.AppId == appId && ownerAppIds.Contains(u.OwnerAppID)).ToListAsync();
            var deleted = new List<PatOwnerApp>();
            var errors = new List<string>();

            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                var appOwner = appOwners.FirstOrDefault(o => o.OwnerAppID == owner.OwnerAppId);

                try
                {
                    Guard.Against.RecordNotFound(appOwner != null);

                    if (appOwner != null)
                    {
                        Guard.Against.NoRecordPermission(await ValidateEntityFilter(appOwner.OwnerID));

                        owner.OwnerID = appOwner.OwnerID;
                        owner.Percentage = appOwner.Percentage;
                        deleted.Add(appOwner);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, owner.AppId.ToString(), owner.OwnerAppId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            await _patOwnerAppService.Update(appId, _user.GetUserName(), new List<PatOwnerApp>(), new List<PatOwnerApp>(), deleted);
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
