using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;

namespace R10.Core.Services
{
    public class TmkCostTrackingService: EntityService<TmkCostTrack>, ICostTrackingService<TmkCostTrack>
    {
        private readonly ITmkTrademarkService _trademarkService;
        private readonly ISystemSettings<TmkSetting> _settings;

        public TmkCostTrackingService(
            ICPiDbContext cpiDbContext,
            ITmkTrademarkService trademarkService,
            ISystemSettings<TmkSetting> settings,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _trademarkService = trademarkService;
            _settings = settings;
        }

        public override IQueryable<TmkCostTrack> QueryableList
        {
            get
            {
                var costTrackings = base.QueryableList;

                //use trademark entity filter and resp office
                //unless responsible agent is used as entity filter
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                    costTrackings = costTrackings.Where(c => _trademarkService.TmkTrademarks.Any(t => t.TmkId == c.TmkId));

                //if (_user.HasRespOfficeFilter(SystemType.Trademark))
                //    costTrackings = costTrackings.Where(RespOfficeFilter());

                //if (_user.HasEntityFilter())
                //    costTrackings = costTrackings.Where(EntityFilter());

                return costTrackings;
            }
        }

        //protected Expression<Func<TmkCostTrack, bool>> RespOfficeFilter()
        //{
        //    return a => _cpiDbContext.GetEntityFilterRepository().CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.Trademark && a.TmkTrademark.RespOffice == r.RespOffice);
        //}

        //protected Expression<Func<TmkCostTrack, bool>> EntityFilter()
        //{
        //    switch (_user.GetEntityFilterType())
        //    {
        //        case CPiEntityType.Agent:
        //            //use trademark agent for entity filter
        //            //return ct => UserEntityFilters.Any(f => f.UserId == userId && f.EntityId == ct.AgentID);
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.TmkTrademark.AgentID);

        //        case CPiEntityType.Client:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.TmkTrademark.ClientID);

        //        case CPiEntityType.Attorney:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && (f.EntityId == ct.TmkTrademark.Attorney1ID || f.EntityId == ct.TmkTrademark.Attorney2ID || f.EntityId == ct.TmkTrademark.Attorney3ID));

        //        case CPiEntityType.Owner:
        //            return ct => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == ct.TmkTrademark.OwnerID);
        //    }
        //    return ct => true;
        //}

        public override async Task<TmkCostTrack> GetByIdAsync(int costTrackId)
        {
            return await QueryableList.SingleOrDefaultAsync(ct => ct.CostTrackId == costTrackId);
        }

        public override async Task Add(TmkCostTrack costTracking)
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

        public override async Task Update(TmkCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingModify);
            await ValidateCostTracking(costTracking);
            await base.Update(costTracking);
        }

        public override async Task UpdateRemarks(TmkCostTrack costTracking)
        {
            var updated = await GetByIdAsync(costTracking.CostTrackId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);
            //await ValidateCostTracking(updated);

            updated.tStamp = costTracking.tStamp;

            _cpiDbContext.GetRepository<TmkCostTrack>().Attach(updated);
            updated.Remarks = costTracking.Remarks;
            updated.UpdatedBy = costTracking.UpdatedBy;
            updated.LastUpdate = costTracking.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(TmkCostTrack costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingDelete);
            await base.Delete(costTracking);
        }

        private async Task ValidatePermission(TmkCostTrack costTracking, List<string> roles)
        {
            var costTrackId = costTracking.CostTrackId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
            {
                var item = new KeyValuePair<int, string>();
                if (costTrackId > 0)
                {
                    item = (await QueryableList.Where(c => c.CostTrackId == costTrackId)
                                    .Select(c => new { c.TmkId, c.TmkTrademark.RespOffice })
                                    .ToDictionaryAsync(c => c.TmkId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    costTracking.SubCase = costTracking.SubCase ?? "";
                    item = (await _trademarkService.TmkTrademarks
                                    .Where(ca => ca.CaseNumber == costTracking.CaseNumber && ca.Country == costTracking.Country && ca.SubCase == costTracking.SubCase)
                                    .Select(c => new { c.TmkId, c.RespOffice })
                                    .ToDictionaryAsync(c => c.TmkId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Trademark, roles, respOfc));
        }

        protected async Task ValidateCostTracking(TmkCostTrack costTracking)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Agent)
            {
                var agentLabel = settings.LabelAgent;
                //use trademark agent for entity filter, allow blank
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

            var trademark = await _trademarkService.TmkTrademarks
                .Where(ca =>
                    ca.CaseNumber == costTracking.CaseNumber &&
                    ca.Country == costTracking.Country &&
                    ca.SubCase == costTracking.SubCase)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(trademark?.TmkId > 0, $"{caseNumberLabel}/Country/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.Trademark))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Trademark, trademark.RespOffice, CPiPermissions.CostTrackingModify), $"{caseNumberLabel}/Country/Sub Case");

            costTracking.TmkId = trademark.TmkId;

            //todo: update country app last update stamp?
        }

        public async Task<bool> CanModifyAgent(int agentId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Agent && agentId > 0)
                return await base.EntityFilterAllowed(agentId);
            else
                return true;
        }

        private async Task AddCustomDocFolder(TmkCostTrack costTracking)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                _cpiDbContext.GetRepository<TmkCostTrack>().Add(costTracking);
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
