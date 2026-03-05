using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;
using R10.Core.DTOs;
using R10.Core.Identity;

namespace R10.Core.Interfaces
{
    public interface IPatWorkflowService
    {
        Task AddWorkflow(PatWorkflow workflow);
        Task UpdateWorkflow(PatWorkflow workflow);
        Task DeleteWorkflow(PatWorkflow workflow);
        Task UpdateChild<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<PatWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteWorkflowAction(int parentId, string userName, byte[] tStamp, IEnumerable<PatWorkflowAction> deleted);
        Task UpdateActionParameter<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;

        Task<List<PatWorkflowAction>> GetWorkflowActions(int WrkId);
        Task<PatWorkflowAction> GetWorkflowAction(int ActId);
        Task WorkflowActionUpdate(PatWorkflowAction workflowAction);
        Task ReorderWorkflowAction(int id, string userName, int newIndex);

        IQueryable<PatWorkflow> PatWorkflows { get; }
        IQueryable<PatWorkflowAction> PatWorkflowActions { get; }
        IQueryable<PatWorkflowActionParameter> PatWorkflowActionParameters { get; }
        IQueryable<CPiRespOffice> CPiRespOffices { get; }
        Task <List<LookupDescDTO>> GetActionTypes();
        Task<List<LookupDescDTO>> GetCostTypes();
        IQueryable<SystemScreen> GetSystemScreens(string systemType);

        Task SaveWorkflowActionAttachmentFilter(int actId, string filter,string userName);
        
    }
}
