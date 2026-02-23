using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
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
using System.Threading.Tasks;
using System.Transactions;

namespace R10.Core.Services.GeneralMatter
{
    public class GMCostTrackingService : EntityService<GMCostTrack>, ICostTrackingService<GMCostTrack>
    {
        private readonly IGMMatterService _matterService;
        private readonly ISystemSettings<GMSetting> _settings;

        public GMCostTrackingService(
            ICPiDbContext cpiDbContext,
            IGMMatterService matterService,
            ISystemSettings<GMSetting> settings,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _matterService = matterService;
            _settings = settings;
        }

        public override IQueryable<GMCostTrack> QueryableList
        {
            get
            {
                var costTrackings = base.QueryableList;

                //use gen matter entity filter and resp office
                //unless responsible agent is used as entity filter
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
                    costTrackings = costTrackings.Where(c => _matterService.QueryableList.Any(gm => gm.MatId == c.MatId));

                //if (_user.HasRespOfficeFilter(SystemType.GeneralMatter))
                //    costTrackings = costTrackings.Where(RespOfficeFilter());

                //if (_user.HasEntityFilter())
                //    costTrackings = costTrackings.Where(EntityFilter());

                return costTrackings;
            }
        }

        //private Expression<Func<GMCostTrack, bool>> RespOfficeFilter()
        //{
        //    return a => CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.GeneralMatter && a.GMMatter.RespOffice == r.RespOffice);
        //}

        //private Expression<Func<GMCostTrack, bool>> EntityFilter()
        //{
        //    switch (_user.GetEntityFilterType())
        //    {
        //        case CPiEntityType.Agent:
        //            //use matter agent for entity filter
        //            //return ct => UserEntityFilters.Any(f => f.UserId == userId && f.EntityId == ct.AgentID);
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.GMMatter.AgentID);

        //        case CPiEntityType.Client:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.GMMatter.ClientID);

        //        case CPiEntityType.Attorney:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && ct.GMMatter.Attorneys.Any(i => i.AttorneyID == f.EntityId));
        //    }
        //    return ct => true;
        //}

        public override async Task<GMCostTrack> GetByIdAsync(int costTrackId)
        {
            return await QueryableList.SingleOrDefaultAsync(ct => ct.CostTrackId == costTrackId);
        }

        public override async Task Add(GMCostTrack costTracking)
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

        public override async Task Update(GMCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingModify);
            await ValidateCostTracking(costTracking);
            await base.Update(costTracking);
        }

        public override async Task UpdateRemarks(GMCostTrack costTracking)
        {
            var updated = await GetByIdAsync(costTracking.CostTrackId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);
            //await ValidateCostTracking(updated);

            updated.tStamp = costTracking.tStamp;

            _cpiDbContext.GetRepository<GMCostTrack>().Attach(updated);
            updated.Remarks = costTracking.Remarks;
            updated.UpdatedBy = costTracking.UpdatedBy;
            updated.LastUpdate = costTracking.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(GMCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingDelete);
            await base.Delete(costTracking);
        }

        private async Task ValidatePermission(GMCostTrack costTracking, List<string> roles)
        {
            var costTrackId = costTracking.CostTrackId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
            {
                var item = new KeyValuePair<int, string>();
                if (costTrackId > 0)
                {
                    item = (await QueryableList.Where(c => c.CostTrackId == costTrackId)
                                    .Select(c => new { c.MatId, c.GMMatter.RespOffice })
                                    .ToDictionaryAsync(c => c.MatId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    costTracking.SubCase = costTracking.SubCase ?? "";
                    item = (await _matterService.QueryableList
                                    .Where(m => m.CaseNumber == costTracking.CaseNumber && m.SubCase == costTracking.SubCase)
                                    .Select(c => new { c.MatId, c.RespOffice })
                                    .ToDictionaryAsync(c => c.MatId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, roles, respOfc));
        }

        private async Task ValidateCostTracking(GMCostTrack costTracking)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Agent)
            {
                var agentLabel = settings.LabelAgent;
                //use matter agent for entity filter, allow blank
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

            var matter = await _matterService.QueryableList
                .Where(ca =>
                    ca.CaseNumber == costTracking.CaseNumber &&
                    ca.SubCase == costTracking.SubCase)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(matter?.MatId > 0, $"{caseNumberLabel}/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.GeneralMatter))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.GeneralMatter, matter.RespOffice, CPiPermissions.CostTrackingModify), $"{caseNumberLabel}/Sub Case");

            costTracking.MatId = matter.MatId;

            //todo: update country app last update stamp?
        }

        public async Task<bool> CanModifyAgent(int agentId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Agent && agentId > 0)
                return await base.EntityFilterAllowed(agentId);
            else
                return true;
        }

        private async Task AddCustomDocFolder(GMCostTrack costTracking)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                _cpiDbContext.GetRepository<GMCostTrack>().Add(costTracking);
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
