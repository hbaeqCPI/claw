using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class PatCostTrackingService: EntityService<PatCostTrack>, ICostTrackingService<PatCostTrack>
    {
        private readonly ICountryApplicationService _countryAppService;
        private readonly ISystemSettings<PatSetting> _settings;

        public PatCostTrackingService(
            ICPiDbContext cpiDbContext,
            ICountryApplicationService countryAppService,
            ISystemSettings<PatSetting> settings,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _countryAppService = countryAppService;
            _settings = settings;
        }

        private bool IsMultipleOwners => _settings.GetSetting().Result.IsMultipleOwnerOn;

        public override IQueryable<PatCostTrack> QueryableList
        {
            get
            {
                var costTrackings = base.QueryableList;
                
                //use country app entity filter and resp office
                //unless responsible agent is used as entity filter
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl())
                    costTrackings = costTrackings.Where(c => _countryAppService.CountryApplications.Any(ca => ca.AppId == c.AppId));

                return costTrackings;
            }
        }

        //private Expression<Func<PatCostTrack, bool>> RespOfficeFilter()
        //{
        //    return a => CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.Patent && a.CountryApplication.RespOffice == r.RespOffice);
        //}

        //private Expression<Func<PatCostTrack, bool>> EntityFilter()
        //{
        //    switch (_user.GetEntityFilterType())
        //    {
        //        case CPiEntityType.Agent:
        //            //use country app agent for entity filter
        //            //return ct => UserEntityFilters.Any(f => f.UserId == userId && f.EntityId == ct.AgentID);
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.CountryApplication.AgentID);

        //        case CPiEntityType.Client:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.CountryApplication.Invention.ClientID);

        //        case CPiEntityType.Inventor:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && ct.CountryApplication.Inventors.Any(i => i.InventorID == f.EntityId));

        //        case CPiEntityType.Attorney:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && (f.EntityId == ct.CountryApplication.Invention.Attorney1ID || f.EntityId == ct.CountryApplication.Invention.Attorney2ID || f.EntityId == ct.CountryApplication.Invention.Attorney3ID));

        //        case CPiEntityType.Owner:
        //            if (IsMultipleOwners)
        //                return ct => UserEntityFilters.Any(f => f.UserId == UserId && ct.CountryApplication.Owners.Any(ao => ao.OwnerID == f.EntityId));
        //            else
        //                return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.CountryApplication.OwnerID);
        //    }
        //    return ct => true;
        //}

        public override async Task<PatCostTrack> GetByIdAsync(int costTrackId)
        {
            return await QueryableList.SingleOrDefaultAsync(ct => ct.CostTrackId == costTrackId);
        }

        public override async Task Add(PatCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingModify);
            await ValidateCostTracking(costTracking);
            if (costTracking.DocFolders != null)
            {
                await AddCustomDocFolder(costTracking);
            }
            else
                await base.Add(costTracking);
        }

        public override async Task Update(PatCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingModify);
            await ValidateCostTracking(costTracking);
            await base.Update(costTracking);
        }

        public override async Task UpdateRemarks(PatCostTrack costTracking)
        {
            var updated = await GetByIdAsync(costTracking.CostTrackId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = costTracking.tStamp;

            _cpiDbContext.GetRepository<PatCostTrack>().Attach(updated);
            updated.Remarks = costTracking.Remarks;
            updated.UpdatedBy = costTracking.UpdatedBy;
            updated.LastUpdate = costTracking.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(PatCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingDelete);
            await base.Delete(costTracking);
        }

        private async Task ValidatePermission(PatCostTrack costTracking, List<string> roles)
        {
            var costTrackId = costTracking.CostTrackId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent))
            {
                var item = new KeyValuePair<int, string>();
                if (costTrackId > 0)
                {
                    item = (await QueryableList.Where(c => c.CostTrackId == costTrackId)
                                    .Select(c => new { c.AppId, c.CountryApplication.RespOffice })
                                    .ToDictionaryAsync(c => c.AppId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    costTracking.SubCase = costTracking.SubCase ?? "";
                    item = (await _countryAppService.CountryApplications
                                    .Where(ca => ca.CaseNumber == costTracking.CaseNumber && ca.Country == costTracking.Country && ca.SubCase == costTracking.SubCase)
                                    .Select(c => new { c.AppId, c.RespOffice })
                                    .ToDictionaryAsync(c => c.AppId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOfc));
        }

        private async Task ValidateCostTracking(PatCostTrack costTracking)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Agent)
            {
                var agentLabel = settings.LabelAgent;
                //use country app agent for entity filter, allow blank
                //Guard.Against.Null(costTracking.AgentID, agentLabel);

                int costTrackId = costTracking.CostTrackId;
                int newAgentId = costTracking.AgentID ?? 0;
                if (costTrackId == 0)
                {
                    if (newAgentId > 0)
                        Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(newAgentId), agentLabel);
                }
                else
                {
                    //check previous value when updating
                    var oldCostTracking = await GetByIdAsync(costTrackId);
                    int oldAgentId = oldCostTracking.AgentID ?? 0;

                    if (oldAgentId != newAgentId)
                    {
                        if (oldAgentId > 0)
                            Guard.Against.NoFieldPermission(await base.EntityFilterAllowed(oldAgentId), agentLabel);

                        if (newAgentId > 0)
                            Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(newAgentId), agentLabel);
                    }
                }
            }

            costTracking.SubCase = costTracking.SubCase ?? "";
            costTracking.ExchangeRate = costTracking.ExchangeRate > 0 ? costTracking.ExchangeRate : 1;
            costTracking.AllowanceRate = costTracking.AllowanceRate > 0 ? costTracking.AllowanceRate : 0;

            var countryApp = await _countryAppService.CountryApplications
                .Where(ca =>
                    ca.CaseNumber == costTracking.CaseNumber &&
                    ca.Country == costTracking.Country &&
                    ca.SubCase == costTracking.SubCase)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(countryApp?.AppId > 0, $"{caseNumberLabel}/Country/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.Patent))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, countryApp.RespOffice, CPiPermissions.CostTrackingModify), $"{caseNumberLabel}/Country/Sub Case");

            costTracking.AppId = countryApp.AppId;

            //todo: update country app last update stamp?
        }

        public async Task<bool> CanModifyAgent(int agentId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Agent && agentId > 0)
                return await base.EntityFilterAllowed(agentId);
            else
                return true;
        }

        private async Task AddCustomDocFolder(PatCostTrack costTracking)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                _cpiDbContext.GetRepository<PatCostTrack>().Add(costTracking);
                await _cpiDbContext.SaveChangesAsync();

                costTracking.DocFolders.ForEach(f => {
                    f.CreatedBy = costTracking.CreatedBy;
                    f.UpdatedBy = costTracking.UpdatedBy;
                    f.DateCreated = costTracking.DateCreated;
                    f.LastUpdate = costTracking.LastUpdate;
                    f.DataKeyValue = costTracking.CostTrackId;
                    f.FolderId = 0;
                    f.DocDocuments.ForEach(d => {
                        d.DocId = 0;
                        d.FolderId = 0;
                        d.CreatedBy = costTracking.CreatedBy;
                        d.UpdatedBy = costTracking.UpdatedBy;
                        d.DateCreated = costTracking.DateCreated;
                        d.LastUpdate = costTracking.LastUpdate;
                    });

                });
                _cpiDbContext.GetRepository<DocFolder>().Add(costTracking.DocFolders);
                await _cpiDbContext.SaveChangesAsync();
                scope.Complete();
            }
        }
    }
}
