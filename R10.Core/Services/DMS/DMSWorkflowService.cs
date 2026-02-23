using System;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.DMS;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System.Linq.Expressions;
using System.Security.Claims;
using R10.Core.Entities.Patent;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Exceptions;
using R10.Core.DTOs;
using System.Transactions;
using System.ComponentModel;

namespace R10.Core.Services
{
    public class DMSWorkflowService : IDMSWorkflowService
    {
        private readonly IApplicationDbContext _repository;

        public DMSWorkflowService(IApplicationDbContext repository)

        {
            _repository = repository;
        }

        public async Task AddWorkflow(DMSWorkflow workflow)
        {
            _repository.DMSWorkflows.Add(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateWorkflow(DMSWorkflow workflow)
        {
            _repository.DMSWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        //public async Task UpdateWorkflowRemarks(DMSWorkflow workflow)
        //{
        //    var entity = _repository.DMSWorkflows.Attach(workflow);
        //    entity.Property(c => c.UserRemarks).IsModified = true;
        //    entity.Property(c => c.UpdatedBy).IsModified = true;
        //    entity.Property(c => c.LastUpdate).IsModified = true;
        //    await _repository.SaveChangesAsync();
        //}

        public async Task DeleteWorkflow(DMSWorkflow workflow)
        {
            _repository.DMSWorkflows.Remove(workflow);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<T> updated, IEnumerable<DMSWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity
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
                _repository.Set<DMSWorkflowAction>().AddRange(added);
            }                

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        #region WorkflowAction

        public async Task DeleteWorkflowAction(int parentId, string userName, IEnumerable<DMSWorkflowAction> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, new List<DMSWorkflowAction>(), new List<DMSWorkflowAction>(), deleted);
            }
        }

        public async Task<List<DMSWorkflowAction>> GetWorkflowActions(int workflowId)
        {
            return await _repository.DMSWorkflowActions.Where(c => c.WrkId == workflowId).AsNoTracking().ToListAsync();
        }

        public async Task<DMSWorkflowAction> GetWorkflowAction(int actId)
        {
            var workflowAction = await _repository.DMSWorkflowActions.Where(c => c.ActId == actId)
                                              .AsNoTracking().Include(c => c.DMSWorkflow).FirstOrDefaultAsync();
            return workflowAction;
        }

        public async Task WorkflowActionUpdate(DMSWorkflowAction workflowAction)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (workflowAction.ActId > 0)
                {
                    _repository.DMSWorkflowActions.Update(workflowAction);
                }
                else
                {
                    _repository.DMSWorkflowActions.Add(workflowAction);
                }

                await UpdateParentStampsAsync(workflowAction.WrkId, workflowAction.UpdatedBy);

                await _repository.SaveChangesAsync(); //we need to get the actId

                scope.Complete();
            }
        }

        public async Task ReorderWorkflowAction(int id, string userName, int newIndex)
        {
            var workflowAction = await DMSWorkflowActions.SingleOrDefaultAsync(a => a.ActId == id);
            Guard.Against.NoRecordPermission(workflowAction != null);
            workflowAction.UpdatedBy = userName;
            workflowAction.LastUpdate = DateTime.Now;

            int wrkId = workflowAction.WrkId;
            int oldIndex = workflowAction.OrderOfEntry;

            var workflow = await DMSWorkflows.Where(w => w.WrkId == wrkId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(workflow != null);
            workflow.UpdatedBy = workflowAction.UpdatedBy;
            workflow.LastUpdate = workflowAction.LastUpdate;

            List<DMSWorkflowAction> workflowActions = new List<DMSWorkflowAction>();
            if (oldIndex > newIndex)
            {
                workflowActions = await DMSWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                workflowActions = await DMSWorkflowActions.Where(w => w.WrkId == wrkId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                workflowActions.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            workflowAction.OrderOfEntry = newIndex;
            workflowActions.Add(workflowAction);

            //_repository.DMSWorkflowActions.AsNoTracking().Update(workflowActions);
            _repository.Set<DMSWorkflowAction>().UpdateRange(workflowActions);
            _repository.DMSWorkflows.Update(workflow);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetActionNextOrderOfEntry(int wrkId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await DMSWorkflowActions.Where(ma => ma.WrkId == wrkId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        #endregion

        public IQueryable<DMSWorkflow> DMSWorkflows => _repository.DMSWorkflows.AsNoTracking();
        public IQueryable<DMSWorkflowAction> DMSWorkflowActions => _repository.DMSWorkflowActions;

        protected async Task UpdateParentStampsAsync(int workflowId, string userName)
        {
            var workflow = await _repository.DMSWorkflows.Where(w => w.WrkId == workflowId).FirstOrDefaultAsync();
            //var workflow = new DMSWorkflow() { WrkId = workflowId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            workflow.UpdatedBy = userName;
            workflow.LastUpdate = DateTime.Now;
            
            var entity = _repository.DMSWorkflows.Attach(workflow);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }

        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            var list = await _repository.DelegationEmailDTO.FromSqlInterpolated($@"Select Distinct DelegationId,AssignedBy,AssignedTo,FirstName,LastName From
                                (Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From tblCPiGroups g Inner Join tblCPiUserGroups ug on g.Id=ug.GroupId Inner Join tblCPIUsers u on u.Id=ug.UserId Inner Join tblDMSDueDateDelegation ddd on ddd.GroupId=ug.GroupId Union
                                 Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From  tblCPIUsers u  Inner Join tblDMSDueDateDelegation ddd on ddd.UserId=u.Id
                                ) t Where t.DelegationId={delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        #region Attachment filter
        public async Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName)
        {
            await _repository.DMSWorkflowActions.Where(a => a.ActId == actId)
                                                        .ExecuteUpdateAsync(a => a.SetProperty(a => a.AttachmentFilter, a => filter)
                                                            .SetProperty(a => a.UpdatedBy, a => userName)
                                                            .SetProperty(a => a.LastUpdate, p => DateTime.Now));
        }
        #endregion
    }
}

