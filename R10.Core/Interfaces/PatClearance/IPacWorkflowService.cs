using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.PatClearance;

namespace R10.Core.Interfaces
{
    public interface IPacWorkflowService
    {
        Task AddWorkflow(PacWorkflow workflow);
        Task UpdateWorkflow(PacWorkflow workflow);
        Task DeleteWorkflow(PacWorkflow workflow);
        Task UpdateChild<T>(int parentId, string userName, IEnumerable<T> updated, IEnumerable<PacWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteWorkflowAction(int parentId, string userName, IEnumerable<PacWorkflowAction> deleted);


        Task<List<PacWorkflowAction>> GetWorkflowActions(int WrkId);
        Task<PacWorkflowAction> GetWorkflowAction(int ActId);
        Task WorkflowActionUpdate(PacWorkflowAction workflowAction);
        Task ReorderWorkflowAction(int id, string userName, int newIndex);

        IQueryable<PacWorkflow> PacWorkflows { get; }
        IQueryable<PacWorkflowAction> PacWorkflowActions { get; }

        Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName);
    }
}
