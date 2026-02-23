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
    public class PatCostTrackingInvService : EntityService<PatCostTrackInv>, ICostTrackingService<PatCostTrackInv>
    {
        private readonly IInventionService _inventionService;
        private readonly ISystemSettings<PatSetting> _settings;

        public PatCostTrackingInvService(
            ICPiDbContext cpiDbContext,
            IInventionService inventionService,
            ISystemSettings<PatSetting> settings,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _inventionService = inventionService;
            _settings = settings;
        }

        private bool IsMultipleOwners => _settings.GetSetting().Result.IsMultipleOwnerOn;

        public override IQueryable<PatCostTrackInv> QueryableList
        {
            get
            {
                var costTrackings = base.QueryableList;

                //use country app entity filter and resp office
                //unless responsible agent is used as entity filter
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl())
                    costTrackings = costTrackings.Where(c => _inventionService.Inventions.Any(i => i.InvId == c.InvId));

                return costTrackings;
            }
        }

        public override async Task<PatCostTrackInv> GetByIdAsync(int costTrackId)
        {
            return await QueryableList.SingleOrDefaultAsync(ct => ct.CostTrackInvId == costTrackId);
        }

        public override async Task Add(PatCostTrackInv costTracking)
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

        public override async Task Update(PatCostTrackInv costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingModify);
            await ValidateCostTracking(costTracking);
            await base.Update(costTracking);
        }

        public override async Task UpdateRemarks(PatCostTrackInv costTracking)
        {
            var updated = await GetByIdAsync(costTracking.CostTrackInvId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = costTracking.tStamp;

            _cpiDbContext.GetRepository<PatCostTrackInv>().Attach(updated);
            updated.Remarks = costTracking.Remarks;
            updated.UpdatedBy = costTracking.UpdatedBy;
            updated.LastUpdate = costTracking.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(PatCostTrackInv costTracking)
        {
            await ValidatePermission(costTracking, CPiPermissions.CostTrackingDelete);
            await base.Delete(costTracking);
        }

        private async Task ValidatePermission(PatCostTrackInv costTracking, List<string> roles)
        {
            var costTrackId = costTracking.CostTrackInvId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent))
            {
                var item = new KeyValuePair<int, string>();
                if (costTrackId > 0)
                {
                    item = (await QueryableList.Where(c => c.CostTrackInvId == costTrackId)
                                    .Select(c => new { c.InvId, c.Invention.RespOffice })
                                    .ToDictionaryAsync(c => c.InvId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    item = (await _inventionService.Inventions
                                    .Where(i => i.CaseNumber == costTracking.CaseNumber)
                                    .Select(c => new { c.InvId, c.RespOffice })
                                    .ToDictionaryAsync(c => c.InvId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOfc));
        }

        private async Task ValidateCostTracking(PatCostTrackInv costTracking)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Agent)
            {
                var agentLabel = settings.LabelAgent;
                //use country app agent for entity filter, allow blank
                //Guard.Against.Null(costTracking.AgentID, agentLabel);

                int costTrackId = costTracking.CostTrackInvId;
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

            costTracking.ExchangeRate = costTracking.ExchangeRate > 0 ? costTracking.ExchangeRate : 1;

            var invention = await _inventionService.Inventions
                .Where(i =>
                    i.CaseNumber == costTracking.CaseNumber)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(invention?.InvId > 0, $"{caseNumberLabel}");

            if (_user.IsRespOfficeOn(SystemType.Patent))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, invention.RespOffice, CPiPermissions.CostTrackingModify), $"{caseNumberLabel}");

            costTracking.InvId = invention.InvId;

            //todo: update country app last update stamp?
        }

        public async Task<bool> CanModifyAgent(int agentId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Agent && agentId > 0)
                return await base.EntityFilterAllowed(agentId);
            else
                return true;
        }

        private async Task AddCustomDocFolder(PatCostTrackInv costTracking)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                _cpiDbContext.GetRepository<PatCostTrackInv>().Add(costTracking);
                await _cpiDbContext.SaveChangesAsync();

                costTracking.DocFolders.ForEach(f => {
                    f.CreatedBy = costTracking.CreatedBy;
                    f.UpdatedBy = costTracking.UpdatedBy;
                    f.DateCreated = costTracking.DateCreated;
                    f.LastUpdate = costTracking.LastUpdate;
                    f.DataKeyValue = costTracking.CostTrackInvId;
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