using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;
using R10.Core.DTOs;
using R10.Core.Identity;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface ITmkWorkflowService
    {
        Task AddWorkflow(TmkWorkflow workflow);
        Task UpdateWorkflow(TmkWorkflow workflow);
        Task DeleteWorkflow(TmkWorkflow workflow);
        Task UpdateChild<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<TmkWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteWorkflowAction(int parentId, string userName, byte[] tStamp, IEnumerable<TmkWorkflowAction> deleted);
        Task UpdateActionParameter<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;

        Task<List<TmkWorkflowAction>> GetWorkflowActions(int WrkId);
        Task<TmkWorkflowAction> GetWorkflowAction(int ActId);
        Task WorkflowActionUpdate(TmkWorkflowAction workflowAction);
        Task ReorderWorkflowAction(int id, string userName, int newIndex);

        IQueryable<TmkWorkflow> TmkWorkflows { get; }
        IQueryable<TmkWorkflowAction> TmkWorkflowActions { get; }
        IQueryable<TmkWorkflowActionParameter> TmkWorkflowActionParameters { get; }
        IQueryable<CPiRespOffice> CPiRespOffices { get; }
        Task <List<LookupDescDTO>> GetActionTypes();
        
        Task<List<LookupDescDTO>> GetCostTypes();
        IQueryable<SystemScreen> GetSystemScreens(string systemType);

        Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName);
    }
}
