using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSWorkflowService
    {
        Task AddWorkflow(DMSWorkflow workflow);
        Task UpdateWorkflow(DMSWorkflow workflow);
        Task DeleteWorkflow(DMSWorkflow workflow);
        Task UpdateChild<T>(int parentId, string userName, IEnumerable<T> updated, IEnumerable<DMSWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteWorkflowAction(int parentId, string userName, IEnumerable<DMSWorkflowAction> deleted);


        Task<List<DMSWorkflowAction>> GetWorkflowActions(int WrkId);
        Task<DMSWorkflowAction> GetWorkflowAction(int ActId);
        Task WorkflowActionUpdate(DMSWorkflowAction workflowAction);
        Task ReorderWorkflowAction(int id, string userName, int newIndex);

        IQueryable<DMSWorkflow> DMSWorkflows { get; }
        IQueryable<DMSWorkflowAction> DMSWorkflowActions { get; }
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);

        Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName);
    }
}
