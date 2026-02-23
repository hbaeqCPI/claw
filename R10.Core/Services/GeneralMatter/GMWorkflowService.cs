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
using R10.Core.Entities.GeneralMatter;
using R10.Core.Identity;
using R10.Core.Entities.Trademark;

namespace R10.Core.Services
{
    public class GMWorkflowService : IGMWorkflowService
    {
        private readonly IApplicationDbContext _repository;

        public GMWorkflowService(IApplicationDbContext repository)

        {
            _repository = repository;
        }

        public async Task AddWorkflow(GMWorkflow workflow)
        {
            _repository.GMWorkflows.Add(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateWorkflow(GMWorkflow workflow)
        {
            _repository.GMWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteWorkflow(GMWorkflow workflow)
        {
            _repository.GMWorkflows.Remove(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<GMWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity
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
                _repository.Set<GMWorkflowAction>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName, tStamp);
            await _repository.SaveChangesAsync();
        }

        #region WorkflowAction

        public async Task DeleteWorkflowAction(int parentId, string userName, byte[] tStamp, IEnumerable<GMWorkflowAction> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, tStamp, new List<GMWorkflowAction>(), new List<GMWorkflowAction>(), deleted);
            }
        }

        public async Task<List<GMWorkflowAction>> GetWorkflowActions(int workflowId)
        {
            return await _repository.GMWorkflowActions.Where(c => c.WrkId == workflowId).AsNoTracking().ToListAsync();
        }

        public async Task<GMWorkflowAction> GetWorkflowAction(int actId)
        {
            var workflowAction = await _repository.GMWorkflowActions.Where(c => c.ActId == actId)
                                              .AsNoTracking().Include(c => c.Workflow).FirstOrDefaultAsync();
            return workflowAction;
        }

        public async Task WorkflowActionUpdate(GMWorkflowAction workflowAction)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (workflowAction.ActId > 0)
                {
                    _repository.GMWorkflowActions.Update(workflowAction);
                }
                else
                {
                    _repository.GMWorkflowActions.Add(workflowAction);
                }

                await UpdateParentStampsAsync(workflowAction.WrkId, workflowAction.UpdatedBy, workflowAction.ParentTStamp);

                await _repository.SaveChangesAsync(); //we need to get the actId

                scope.Complete();
            }
        }

        public async Task ReorderWorkflowAction(int id, string userName, int newIndex)
        {
            var workflowAction = await GMWorkflowActions.SingleOrDefaultAsync(a => a.ActId == id);
            Guard.Against.NoRecordPermission(workflowAction != null);
            workflowAction.UpdatedBy = userName;
            workflowAction.LastUpdate = DateTime.Now;

            int wrkId = workflowAction.WrkId;
            int oldIndex = workflowAction.OrderOfEntry;

            var workflow = await GMWorkflows.Where(w => w.WrkId == wrkId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(workflow != null);
            workflow.UpdatedBy = workflowAction.UpdatedBy;
            workflow.LastUpdate = workflowAction.LastUpdate;

            List<GMWorkflowAction> workflowActions = new List<GMWorkflowAction>();
            if (oldIndex > newIndex)
            {
                workflowActions = await GMWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                workflowActions = await GMWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            workflowAction.OrderOfEntry = newIndex;
            workflowActions.Add(workflowAction);

            //_repository.GMWorkflowActions.AsNoTracking().Update(workflowActions);
            _repository.Set<GMWorkflowAction>().UpdateRange(workflowActions);
            _repository.GMWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetActionNextOrderOfEntry(int wrkId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await GMWorkflowActions.Where(ma => ma.WrkId == wrkId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        #endregion

        public IQueryable<GMWorkflow> GMWorkflows => _repository.GMWorkflows.AsNoTracking();
        public IQueryable<GMWorkflowAction> GMWorkflowActions => _repository.GMWorkflowActions;
        public IQueryable<CPiRespOffice> CPiRespOffices => _repository.CPiRespOffices.AsNoTracking();
        public IQueryable<GMWorkflowActionParameter> GMWorkflowActionParameters => _repository.GMWorkflowActionParameters;

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

        protected async Task UpdateParentStampsAsync(int workflowId, string userName, byte[] tStamp)
        {
            var workflow = await _repository.GMWorkflows.Where(w => w.WrkId == workflowId).FirstOrDefaultAsync();
            //var workflow = new GMWorkflow() { WrkId = workflowId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            workflow.UpdatedBy = userName;
            workflow.LastUpdate = DateTime.Now;
            workflow.tStamp = tStamp;
            var entity = _repository.GMWorkflows.Attach(workflow);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }

        public async Task<List<LookupDTO>> GetActionTypes()
        {
            var list = await _repository.GMActionTypeDTO
               .FromSqlRaw($"Select [Text],cast([Value] as varchar) as [Value] From vwGMActionType Order By [Text]").AsNoTracking()
               .ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetDedocketInstructions()
        {
            var list = await _repository.DeDocketInstructions.Where(d => d.InUse).Select(d => new LookupDTO { Value = d.InstructionId.ToString(), Text = d.Instruction }).ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCostTypes()
        {
            var list = await _repository.GMCostTypes.Select(c => new LookupDTO { Value = c.CostTypeID.ToString(), Text = c.CostType }).ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetQETemplates(string screenType)
        {
            var screen = await _repository.SystemScreens.Where(s => s.SystemType=="G" &&  s.FeatureType == "QE" && s.ScreenName == screenType).FirstOrDefaultAsync();
            if (screen != null)
            {
                var list = await _repository.QEMains.Where(qe => qe.InUse && qe.ScreenId == screen.ScreenId)
                           .Select(t => new LookupDTO { Value = t.QESetupID.ToString(), Text = t.TemplateName }).ToListAsync();
                return list;
            }
            return new List<LookupDTO>();
        }

        public IQueryable<SystemScreen> GetSystemScreens(string systemType)
        {
            return _repository.SystemScreens.Where(e => e.SystemType == systemType && e.FeatureType == "WF");
        }

        #region Attachment filter
        public async Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName)
        {
            await _repository.GMWorkflowActions.Where(a => a.ActId == actId)
                                                        .ExecuteUpdateAsync(a => a.SetProperty(a => a.AttachmentFilter, a => filter)
                                                            .SetProperty(a => a.UpdatedBy, a => userName)
                                                            .SetProperty(a => a.LastUpdate, p => DateTime.Now));
        }
        #endregion

    }
}
