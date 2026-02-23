using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Clearance;

namespace R10.Core.Interfaces
{
    public interface ITmcWorkflowService
    {
        Task AddWorkflow(TmcWorkflow workflow);
        Task UpdateWorkflow(TmcWorkflow workflow);
        Task DeleteWorkflow(TmcWorkflow workflow);
        Task UpdateChild<T>(int parentId, string userName, IEnumerable<T> updated, IEnumerable<TmcWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteWorkflowAction(int parentId, string userName, IEnumerable<TmcWorkflowAction> deleted);


        Task<List<TmcWorkflowAction>> GetWorkflowActions(int WrkId);
        Task<TmcWorkflowAction> GetWorkflowAction(int ActId);
        Task WorkflowActionUpdate(TmcWorkflowAction workflowAction);
        Task ReorderWorkflowAction(int id, string userName, int newIndex);

        IQueryable<TmcWorkflow> TmcWorkflows { get; }
        IQueryable<TmcWorkflowAction> TmcWorkflowActions { get; }

        Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName);
    }
}
