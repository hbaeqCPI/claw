using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.GeneralMatter;
using R10.Core.DTOs;
using R10.Core.Identity;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface IGMWorkflowService
    {
        Task AddWorkflow(GMWorkflow workflow);
        Task UpdateWorkflow(GMWorkflow workflow);
        Task DeleteWorkflow(GMWorkflow workflow);
        Task UpdateChild<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<GMWorkflowAction> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteWorkflowAction(int parentId, string userName, byte[] tStamp, IEnumerable<GMWorkflowAction> deleted);
        Task UpdateActionParameter<T>(int parentId, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;

        Task<List<GMWorkflowAction>> GetWorkflowActions(int WrkId);
        Task<GMWorkflowAction> GetWorkflowAction(int ActId);
        Task WorkflowActionUpdate(GMWorkflowAction workflowAction);
        Task ReorderWorkflowAction(int id, string userName, int newIndex);

        IQueryable<GMWorkflow> GMWorkflows { get; }
        IQueryable<GMWorkflowAction> GMWorkflowActions { get; }
        IQueryable<GMWorkflowActionParameter> GMWorkflowActionParameters { get; }
        IQueryable<CPiRespOffice> CPiRespOffices { get; }
        Task <List<LookupDTO>> GetActionTypes();
        Task<List<LookupDTO>> GetDedocketInstructions();
        Task<List<LookupDTO>> GetCostTypes();
        Task<List<LookupDTO>> GetQETemplates(string screenType);
        IQueryable<SystemScreen> GetSystemScreens(string systemType);

        Task SaveWorkflowActionAttachmentFilter(int actId, string filter, string userName);
    }
}
