using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using System.Transactions;
using R10.Core.Exceptions;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Identity;

namespace R10.Core.Services
{
    public class PatWorkflowService : IPatWorkflowService
    {
        private readonly IApplicationDbContext _repository;

        public PatWorkflowService(IApplicationDbContext repository)

        {
            _repository = repository;
        }

        public async Task AddWorkflow(PatWorkflow workflow)
        {
            _repository.PatWorkflows.Add(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateWorkflow(PatWorkflow workflow)
        {
            _repository.PatWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteWorkflow(PatWorkflow workflow)
        {
            _repository.PatWorkflows.Remove(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<PatWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
                _repository.Set<T>().UpdateRange(updated);

            if (added.Any())
            {
                var startIndex = await GetActionNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<PatWorkflowAction>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName, tStamp);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateActionParameter<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
                _repository.Set<T>().UpdateRange(updated);

            if (added.Any())
            {
                _repository.Set<T>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName, tStamp);
            await _repository.SaveChangesAsync();
        }

        #region WorkflowAction

        public async Task DeleteWorkflowAction(int parentId, string userName, byte[] tStamp, IEnumerable<PatWorkflowAction> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, tStamp, new List<PatWorkflowAction>(), new List<PatWorkflowAction>(), deleted);
            }
        }

        public async Task<List<PatWorkflowAction>> GetWorkflowActions(int workflowId)
        {
            return await _repository.PatWorkflowActions.Where(c => c.WrkId == workflowId).AsNoTracking().ToListAsync();
        }

        public async Task<PatWorkflowAction> GetWorkflowAction(int actId)
        {
            var workflowAction = await _repository.PatWorkflowActions.Where(c => c.ActId == actId)
                                              .AsNoTracking().Include(c => c.Workflow).FirstOrDefaultAsync();
            return workflowAction;
        }

        public async Task WorkflowActionUpdate(PatWorkflowAction workflowAction)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (workflowAction.ActId > 0)
                {
                    _repository.PatWorkflowActions.Update(workflowAction);
                }
                else
                {
                    _repository.PatWorkflowActions.Add(workflowAction);
                }

                await UpdateParentStampsAsync(workflowAction.WrkId, workflowAction.UpdatedBy, workflowAction.ParentTStamp);

                await _repository.SaveChangesAsync(); //we need to get the actId

                scope.Complete();
            }
        }

        public async Task ReorderWorkflowAction(int id, string userName, int newIndex)
        {
            var workflowAction = await PatWorkflowActions.SingleOrDefaultAsync(a => a.ActId == id);
            Guard.Against.NoRecordPermission(workflowAction != null);
            workflowAction.UpdatedBy = userName;
            workflowAction.LastUpdate = DateTime.Now;

            int wrkId = workflowAction.WrkId;
            int oldIndex = workflowAction.OrderOfEntry;

            var workflow = await PatWorkflows.Where(w => w.WrkId == wrkId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(workflow != null);
            workflow.UpdatedBy = workflowAction.UpdatedBy;
            workflow.LastUpdate = workflowAction.LastUpdate;

            List<PatWorkflowAction> workflowActions = new List<PatWorkflowAction>();
            if (oldIndex > newIndex)
            {
                workflowActions = await PatWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                workflowActions = await PatWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            workflowAction.OrderOfEntry = newIndex;
            workflowActions.Add(workflowAction);

            //_repository.PatWorkflowActions.AsNoTracking().Update(workflowActions);
            _repository.Set<PatWorkflowAction>().UpdateRange(workflowActions);
            _repository.PatWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetActionNextOrderOfEntry(int wrkId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await PatWorkflowActions.Where(ma => ma.WrkId == wrkId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        #endregion

        public IQueryable<PatWorkflow> PatWorkflows => _repository.PatWorkflows.AsNoTracking();
        public IQueryable<PatWorkflowAction> PatWorkflowActions => _repository.PatWorkflowActions;
        public IQueryable<PatWorkflowActionParameter> PatWorkflowActionParameters => _repository.PatWorkflowActionParameters;
        public IQueryable<CPiRespOffice> CPiRespOffices => _repository.CPiRespOffices.AsNoTracking();

        protected async Task UpdateParentStampsAsync(int workflowId, string userName, byte[] tStamp)
        {
            var workflow = await _repository.PatWorkflows.Where(w => w.WrkId == workflowId).FirstOrDefaultAsync();
            //var workflow = new PatWorkflow() { WrkId = workflowId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            workflow.UpdatedBy = userName;
            workflow.LastUpdate = DateTime.Now;
            workflow.tStamp = tStamp;
            var entity = _repository.PatWorkflows.Attach(workflow);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }

        public async Task<List<LookupDescDTO>> GetActionTypes()
        {
            var list = await _repository.PatActionTypeDTO
               .FromSqlRaw($"Select [Text],cast([Value] as varchar) as [Value],[Source] as Description From vwPatActionTypeUnion Order By [Text]").AsNoTracking()
               .ToListAsync();
            return list;
        }

        public async Task<List<LookupDescDTO>> GetDedocketInstructions()
        {
            var list = await _repository.DeDocketInstructions.Where(d => d.InUse).Select(d => new LookupDescDTO { Value = d.InstructionId.ToString(), Text = d.Instruction }).ToListAsync();
            return list;
        }

        public async Task<List<LookupDescDTO>> GetCostTypes()
        {
            var list = await _repository.PatCostTypes.Select(c => new LookupDescDTO { Value = c.CostTypeID.ToString(), Text = c.CostType}).ToListAsync();
            return list;
        }

        public async Task<List<LookupDescDTO>> GetQETemplates(string screenType) {
            var screen = await _repository.SystemScreens.Where(s => s.SystemType == "P" && s.FeatureType == "QE" && s.ScreenName == screenType).FirstOrDefaultAsync();
            if (screen != null) {
                var list = await _repository.QEMains.Where(qe=> qe.InUse && qe.ScreenId==screen.ScreenId)
                           .Select(t => new LookupDescDTO { Value = t.QESetupID.ToString(), Text = t.TemplateName }).ToListAsync();
                return list;
            }
            return new List<LookupDescDTO>();
        }

        public IQueryable<SystemScreen> GetSystemScreens(string systemType) {
            return _repository.SystemScreens.Where(e => e.SystemType == systemType && e.FeatureType == "WF");
        }

        #region Attachment filter
        public async Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName) {
            await _repository.PatWorkflowActions.Where(a => a.ActId == actId)
                                                        .ExecuteUpdateAsync(a => a.SetProperty(a => a.AttachmentFilter, a => filter)
                                                            .SetProperty(a => a.UpdatedBy, a => userName)
                                                            .SetProperty(a => a.LastUpdate, p => DateTime.Now));
        }
        #endregion
    }
}
