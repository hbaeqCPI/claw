using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using R10.Core.Entities.Clearance;
using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using System.Transactions;
using R10.Core.Exceptions;

namespace R10.Core.Services
{
    public class TmcWorkflowService : ITmcWorkflowService
    {
        private readonly IApplicationDbContext _repository;

        public TmcWorkflowService(IApplicationDbContext repository)

        {
            _repository = repository;
        }

        public async Task AddWorkflow(TmcWorkflow workflow)
        {
            _repository.TmcWorkflows.Add(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateWorkflow(TmcWorkflow workflow)
        {
            _repository.TmcWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteWorkflow(TmcWorkflow workflow)
        {
            _repository.TmcWorkflows.Remove(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<T> updated, IEnumerable<TmcWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity
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
                _repository.Set<TmcWorkflowAction>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        #region WorkflowAction

        public async Task DeleteWorkflowAction(int parentId, string userName, IEnumerable<TmcWorkflowAction> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, new List<TmcWorkflowAction>(), new List<TmcWorkflowAction>(), deleted);
            }
        }

        public async Task<List<TmcWorkflowAction>> GetWorkflowActions(int workflowId)
        {
            return await _repository.TmcWorkflowActions.Where(c => c.WrkId == workflowId).AsNoTracking().ToListAsync();
        }

        public async Task<TmcWorkflowAction> GetWorkflowAction(int actId)
        {
            var workflowAction = await _repository.TmcWorkflowActions.Where(c => c.ActId == actId)
                                              .AsNoTracking().Include(c => c.Workflow).FirstOrDefaultAsync();
            return workflowAction;
        }

        public async Task WorkflowActionUpdate(TmcWorkflowAction workflowAction)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (workflowAction.ActId > 0)
                {
                    _repository.TmcWorkflowActions.Update(workflowAction);
                }
                else
                {
                    _repository.TmcWorkflowActions.Add(workflowAction);
                }

                await UpdateParentStampsAsync(workflowAction.WrkId, workflowAction.UpdatedBy);

                await _repository.SaveChangesAsync(); //we need to get the actId

                scope.Complete();
            }
        }

        public async Task ReorderWorkflowAction(int id, string userName, int newIndex)
        {
            var workflowAction = await TmcWorkflowActions.SingleOrDefaultAsync(a => a.ActId == id);
            Guard.Against.NoRecordPermission(workflowAction != null);
            workflowAction.UpdatedBy = userName;
            workflowAction.LastUpdate = DateTime.Now;

            int wrkId = workflowAction.WrkId;
            int oldIndex = workflowAction.OrderOfEntry;

            var workflow = await TmcWorkflows.Where(w => w.WrkId == wrkId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(workflow != null);
            workflow.UpdatedBy = workflowAction.UpdatedBy;
            workflow.LastUpdate = workflowAction.LastUpdate;

            List<TmcWorkflowAction> workflowActions = new List<TmcWorkflowAction>();
            if (oldIndex > newIndex)
            {
                workflowActions = await TmcWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                workflowActions = await TmcWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            workflowAction.OrderOfEntry = newIndex;
            workflowActions.Add(workflowAction);

            //_repository.TmcWorkflowActions.AsNoTracking().Update(workflowActions);
            _repository.Set<TmcWorkflowAction>().UpdateRange(workflowActions);
            _repository.TmcWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetActionNextOrderOfEntry(int wrkId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await TmcWorkflowActions.Where(ma => ma.WrkId == wrkId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        #endregion

        public IQueryable<TmcWorkflow> TmcWorkflows => _repository.TmcWorkflows.AsNoTracking();
        public IQueryable<TmcWorkflowAction> TmcWorkflowActions => _repository.TmcWorkflowActions;

        protected async Task UpdateParentStampsAsync(int workflowId, string userName)
        {
            var workflow = await _repository.TmcWorkflows.Where(w => w.WrkId == workflowId).FirstOrDefaultAsync();
            //var workflow = new TmcWorkflow() { WrkId = workflowId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            workflow.UpdatedBy = userName;
            workflow.LastUpdate = DateTime.Now;
            
            var entity = _repository.TmcWorkflows.Attach(workflow);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }

        #region Attachment filter
        public async Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName)
        {
            await _repository.TmcWorkflowActions.Where(a => a.ActId == actId)
                                                        .ExecuteUpdateAsync(a => a.SetProperty(a => a.AttachmentFilter, a => filter)
                                                            .SetProperty(a => a.UpdatedBy, a => userName)
                                                            .SetProperty(a => a.LastUpdate, p => DateTime.Now));
        }
        #endregion
    }
}
