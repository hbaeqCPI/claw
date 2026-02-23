using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSMainService : EntityService<AMSMain>, IAMSMainService
    {
        private readonly ISystemSettings<AMSSetting> _amsSettings;
        private readonly ISystemSettings<PatSetting> _patSettings;

        public AMSMainService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user,
            ISystemSettings<AMSSetting> amsSettings,
            ISystemSettings<PatSetting> patSettings
            ) : base(cpiDbContext, user)
        {
            _amsSettings = amsSettings;
            _patSettings = patSettings;
        }

        public override IQueryable<AMSMain> QueryableList
        {
            get
            {
                var ams = _cpiDbContext.GetRepository<AMSMain>().QueryableList.Where(a => a.CPIDeleteFlag == false);

                if (_user.HasRespOfficeFilter(SystemType.AMS))
                    ams = ams.Where(RespOfficeFilter());

                if (_user.HasEntityFilter())
                {
                    if (_user.IsAMSIntegrated())
                        ams = ams.Where(IntegratedEntityFilter());
                    else
                        ams = ams.Where(EntityFilter());
                }

                return ams;
            }
        }

        private Expression<Func<AMSMain, bool>> RespOfficeFilter()
        {
            return a => CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.AMS && a.CPIClientCode == r.RespOffice && !string.IsNullOrEmpty(r.RespOffice));
        }

        private Expression<Func<AMSMain, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ams.Client.ClientID);

                case CPiEntityType.Agent:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ams.Agent.AgentID);

                case CPiEntityType.Attorney:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ams.Attorney.AttorneyID);

                //DECISION MAKERS
                case CPiEntityType.ContactPerson:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && (ams.Client.ClientContacts.Any(d => d.ContactID == f.EntityId)));

            }
            return ams => true;
        }

        private Expression<Func<AMSMain, bool>> IntegratedEntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == (ams.CountryApplication == null ? ams.Client.ClientID : ams.CountryApplication.Invention.ClientID));

                case CPiEntityType.Agent:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == (ams.CountryApplication == null ? ams.Agent.AgentID : ams.CountryApplication.AgentID));

                case CPiEntityType.Attorney:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == (ams.CountryApplication == null ? ams.Attorney.AttorneyID : ams.CountryApplication.Invention.Attorney1ID));

                //DECISION MAKERS
                case CPiEntityType.ContactPerson:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && (
                        (ams.CountryApplication == null && ams.Client.ClientContacts.Any(d => d.ContactID == f.EntityId)) ||
                        (ams.CountryApplication != null && ams.CountryApplication.Invention.Client.ClientContacts.Any(d => d.ContactID == f.EntityId))
                    ));

                case CPiEntityType.Owner:
                    return ams => UserEntityFilters.Any(f => f.UserId == UserId && ams.CountryApplication != null && ams.CountryApplication.Owners != null && ams.CountryApplication.Owners.Any(o => o.OwnerID == f.EntityId));
            }
            return ams => true;
        }

        public override Task Add(AMSMain entity)
        {
            //return base.Add(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task Delete(AMSMain entity)
        {
            // return base.Delete(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override async Task<AMSMain> GetByIdAsync(int annId)
        {
            return await QueryableList.SingleOrDefaultAsync(ams => ams.AnnID == annId);
        }

        public override async Task Update(AMSMain entity)
        {
            //return base.Update(entity);
            await Update(entity, "");
        }

        public async Task Update(AMSMain entity, string taxSchedChangeReason)
        {
            var amsSettings = await _amsSettings.GetSetting();
            var amsMain = await ValidatePermission(entity.AnnID, CPiPermissions.FullModify);
            amsMain.tStamp = entity.tStamp;

            _cpiDbContext.GetRepository<AMSMain>().Attach(amsMain);
            amsMain.Remarks = entity.Remarks;
            amsMain.UpdatedBy = entity.UpdatedBy;
            amsMain.LastUpdate = entity.LastUpdate;

            if (!(await IsDataOverride()))
            {
                amsMain.CPITitle = entity.CPITitle;
                amsMain.CPIClient = entity.CPIClient;
                amsMain.CPIAgent = entity.CPIAgent;
                amsMain.CPIAttorney = entity.CPIAttorney;
                amsMain.CPIOwner = entity.CPIOwner;
                amsMain.CPIInventors = entity.CPIInventors;
                amsMain.CPITaxSchedule = entity.CPITaxSchedule;
            }

            if (AllowTaxScheduleEdit(amsMain.Country) && (entity.CPITaxSchedule ?? "") != (amsMain.CPITaxSchedule ?? ""))
            {
                Guard.Against.NullOrEmpty(taxSchedChangeReason, "Reason For Changing CPI Tax Schedule");

                _cpiDbContext.GetRepository<AMSTaxSchedHistory>().Update(new AMSTaxSchedHistory()
                {
                    AnnID = entity.AnnID,
                    CPITaxSchedule = entity.CPITaxSchedule,
                    ReasonForChange = taxSchedChangeReason,
                    DateChanged = (DateTime)entity.LastUpdate,
                    CreatedBy = entity.UpdatedBy
                });

                amsMain.CPITaxSchedule = entity.CPITaxSchedule;
            }

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task UpdateRemarks(AMSMain entity)
        {
            var amsMain = await ValidatePermission(entity.AnnID, CPiPermissions.RemarksOnly);
            amsMain.tStamp = entity.tStamp;

            _cpiDbContext.GetRepository<AMSMain>().Attach(amsMain);
            amsMain.Remarks = entity.Remarks;
            amsMain.UpdatedBy = entity.UpdatedBy;
            amsMain.LastUpdate = entity.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<AMSMain> ValidatePermission(int annId, List<string> roles)
        {
            var amsMain = await QueryableList.Where(i => i.AnnID == annId).SingleOrDefaultAsync();
            Guard.Against.NoRecordPermission(amsMain != null);
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.AMS, roles, amsMain.CPIClientCode));

            return amsMain;
        }

        public bool AllowTaxScheduleEdit(string country)
        {
            return (country.ToUpper() == "US");
        }

        /// <summary>
        /// DISABLE FF FIELDS IF EITHER IsAMSIntegrated OR DownloadOverwrite IS TRUE:
        /// CPITitle
        /// CPIClient
        /// CPIAgent
        /// CPIAttorney
        /// CPIOwner
        /// CPIInventors
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsDataOverride()
        {
            var amsSettings = await _amsSettings.GetSetting();
            return _user.IsAMSIntegrated() || amsSettings.DownloadOverwrite;
        }

        public async Task<List<AMSTaxSchedHistory>> GetTaxScheduleHistory(int annId)
        {
            return await _cpiDbContext.GetRepository<AMSTaxSchedHistory>().QueryableList.Where(t => t.AnnID == annId).ToListAsync();
        }

        public async Task<bool> IsProductsOn()
        {
            var amsSettings = await _amsSettings.GetSetting();
            var patSettings = await _patSettings.GetSetting();

            return _user.IsAMSIntegrated() ? amsSettings.IsProductsOn && patSettings.IsProductsOn : amsSettings.IsProductsOn;
        }

        public async Task<bool> CanAccessProducts()
        {
            var canAccessProducts = await CPiUserSystemRoles.Where(s => s.UserId == UserId && CPiPermissions.Products.Contains(s.RoleId)).Select(s => s.SystemId).ToListAsync();
            return await IsProductsOn() && (_user.IsAdmin() || canAccessProducts.Contains(_user.IsAMSIntegrated() ? SystemType.Patent : SystemType.AMS));
        }

        public async Task<bool> IsLicenseesOn()
        {
            var amsSettings = await _amsSettings.GetSetting();
            var patSettings = await _patSettings.GetSetting();

            return _user.IsAMSIntegrated() ? amsSettings.IsLicenseesOn && patSettings.IsLicenseesOn : amsSettings.IsLicenseesOn;
        }

        public async Task<bool> IsPatentScoreOn()
        {
            var patSettings = await _patSettings.GetSetting();

            return _user.IsAMSIntegrated() ? patSettings.IsPatentScoreOn: false;
        }
    }
}
