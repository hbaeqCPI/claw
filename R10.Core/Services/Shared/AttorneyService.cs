using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class AttorneyService : EntityService<Attorney>, IAttorneyService
    {
        protected readonly IInventionService _inventionService;
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly IGMMatterService _gMMatterService;
        protected readonly IDisclosureService _disclosureService;
        protected readonly IPacClearanceService _pacClearanceService;
        protected readonly ITmcClearanceService _tmcClearanceService;

        public AttorneyService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user,
            IInventionService inventionService,
            ITmkTrademarkService trademarkService,
            IGMMatterService gMMatterService,
            IDisclosureService disclosureService,
            IPacClearanceService pacClearanceService,
            ITmcClearanceService tmcClearanceService
            ) : base(cpiDbContext, user)
        {
            _inventionService = inventionService;
            _trademarkService = trademarkService;
            _gMMatterService = gMMatterService;
            _disclosureService = disclosureService;
            _pacClearanceService = pacClearanceService;
            _tmcClearanceService = tmcClearanceService;
        }

        public override IQueryable<Attorney> QueryableList
        {
            get
            {
                var attorneys = base.QueryableList;

                if (_user.GetEntityFilterType() == CPiEntityType.Attorney)
                    attorneys = attorneys.Where(EntityFilter());

                else if (_user.IsSharedLimited())
                    attorneys = attorneys.Where(a =>
                        (_user.IsInSystem(SystemType.Patent) && (
                            _inventionService.QueryableList.Any(i => i.Attorney1ID == a.AttorneyID) ||
                            _inventionService.QueryableList.Any(i => i.Attorney2ID == a.AttorneyID) ||
                            _inventionService.QueryableList.Any(i => i.Attorney3ID == a.AttorneyID) ||
                            _inventionService.QueryableList.Any(i => i.Attorney4ID == a.AttorneyID) ||
                            _inventionService.QueryableList.Any(i => i.Attorney5ID == a.AttorneyID))) ||
                        (_user.IsInSystem(SystemType.Trademark) && (
                            _trademarkService.TmkTrademarks.Any(t => t.Attorney1ID == a.AttorneyID) ||
                            _trademarkService.TmkTrademarks.Any(t => t.Attorney2ID == a.AttorneyID) ||
                            _trademarkService.TmkTrademarks.Any(t => t.Attorney3ID == a.AttorneyID) ||
                            _trademarkService.TmkTrademarks.Any(t => t.Attorney4ID == a.AttorneyID) ||
                            _trademarkService.TmkTrademarks.Any(t => t.Attorney5ID == a.AttorneyID))) ||
                        (_user.IsInSystem(SystemType.GeneralMatter) &&
                            _gMMatterService.QueryableList.Any(g => g.Attorneys.Any(ga => ga.AttorneyID == a.AttorneyID))) ||
                        (_user.IsInSystem(SystemType.DMS) &&
                            _disclosureService.QueryableList.Any(d => d.AttorneyID == a.AttorneyID)) ||
                        (_user.IsInSystem(SystemType.PatClearance) &&
                            _pacClearanceService.QueryableList.Any(p => p.AttorneyID == a.AttorneyID)) ||
                        (_user.IsInSystem(SystemType.SearchRequest) &&
                            _tmcClearanceService.QueryableList.Any(t => t.AttorneyID == a.AttorneyID)));

                return attorneys;
            }
        }

        public IQueryable<Attorney> QueryableListWithoutFilter
        {
            get
            {
                return base.QueryableList;
            }
        }

        protected Expression<Func<Attorney, bool>> EntityFilter()
        {
            return a => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == a.AttorneyID);
        }

        public IQueryable<Attorney> ClearanceQueryableList
        {
            get
            {
                var attorneys = base.QueryableList;

                if (_user.GetEntityFilterType() == CPiEntityType.Attorney)
                    attorneys = attorneys.Where(EntityFilter());                

                return attorneys;
            }
        }

        public override async Task<Attorney> GetByIdAsync(int attorneyID)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.AttorneyID == attorneyID);
        }

        public override async Task Add(Attorney attorney)
        {
            await ValidateAttorney(attorney);

            if (attorney.EMail != null && attorney.EMail.Contains(","))
            {
                attorney.EMail = attorney.EMail.Replace(",", ";");
            }

            await base.Add(attorney);
        }

        public override async Task Delete(Attorney attorney)
        {
            await ValidatePermission(attorney.AttorneyID);
            await base.Delete(attorney);
        }

        public override async Task Update(Attorney attorney)
        {
            await ValidatePermission(attorney.AttorneyID);
            await ValidateAttorney(attorney);

            if (attorney.EMail != null && attorney.EMail.Contains(","))
            {
                attorney.EMail = attorney.EMail.Replace(",", ";");
            }

            //EF.core will not update Attorney field because it is set as principal key in tblAMSMain.CPIAttorney -> tblAttorney.Attorney mapping.
            await _cpiDbContext.GetRepository<Attorney>().UpdateKeyAsync(attorney, "Attorney", "AttorneyID", attorney.AttorneyCode, attorney.AttorneyID);
            await base.Update(attorney);

        }

        public override async Task UpdateRemarks(Attorney attorney)
        {
            await ValidatePermission(attorney.AttorneyID);
            await base.UpdateRemarks(attorney);
        }

        private async Task ValidatePermission(int attorneyID)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(a => a.AttorneyID == attorneyID));
        }

        private async Task ValidateAttorney(Attorney attorney)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(attorney.AttorneyID), "Attorney");
        }

        public async Task<byte[]> SaveReminderOption(int attorneyID, ReminderOption option, bool value, byte[] tStamp, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(attorneyID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Attorney), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AttorneyID));

            updated.tStamp = tStamp;
            _cpiDbContext.GetRepository<Attorney>().Attach(updated);

            if (option == ReminderOption.ReceiveReminderOnline)
                updated.ReceiveReminderOnline = value;
            else if (option == ReminderOption.ReceiveReminderReport)
                updated.ReceiveReminderReport = value;
            else if (option == ReminderOption.ReceivePrepayReminder)
                updated.ReceivePrepayReminder = value;

            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task SaveLastReminderSentDate(int attorneyID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(attorneyID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Attorney), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AttorneyID));

            _cpiDbContext.GetRepository<Attorney>().Attach(updated);

            updated.LastReminderSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastPrepayReminderSentDate(int attorneyID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(attorneyID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Attorney), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AttorneyID));

            _cpiDbContext.GetRepository<Attorney>().Attach(updated);

            updated.LastPrepayReminderSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task SaveLastConfirmationLetterSentDate(int attorneyID, DateTime sentDate, string userName)
        {
            Guard.Against.NoRecordPermission(
                            (await ValidatePermission(SystemType.Shared, CPiPermissions.FullModify, "")) ||
                            (await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, ""))
                            );

            var updated = await GetByIdAsync(attorneyID);

            Guard.Against.NoRecordPermission(updated != null);
            if (EntityType.IsEntityFilterType(typeof(Attorney), _user.GetEntityFilterType()))
                Guard.Against.NoRecordPermission(await base.EntityFilterAllowed(updated.AttorneyID));

            _cpiDbContext.GetRepository<Attorney>().Attach(updated);

            updated.LastConfirmationLetterSentDate = sentDate;
            updated.UpdatedBy = userName;
            updated.LastUpdate = sentDate;

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);
        }

        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            var customFieldSettings = _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList;
            return await customFieldSettings.Where(s => s.TableName == "tblAttorney" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }
    }
}
