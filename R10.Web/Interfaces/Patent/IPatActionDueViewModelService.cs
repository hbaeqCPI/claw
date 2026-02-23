using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IPatActionDueViewModelService
    {

        Task<PatActionDueDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatActionDue> actionsDue, List<QueryFilterViewModel> dueDateFilters);
        IQueryable<PatActionDue> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<PatActionDue> actionsDue);
        PatActionDue ConvertViewModelToActionDue(PatActionDueDetailViewModel viewModel);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<PatActionDue> actionsDue, string value);

        Task<List<PatActionDueAppInfoViewModel>> GetAppInfoList(string caseNumber, string country, string subCase);

        IQueryable<PatActionDueAppInfoViewModel> AppInfo { get; }

        #region Workflow
        Task<List<WorkflowEmailViewModel>> NewOrCompletedActionWorkflow(PatActionDue actionDue, string? emailUrl, bool newAction);
        Task<List<WorkflowEmailViewModel>> NewDedocketInstructionWorkflow(IList<ActionDueViewModel> updated, string? emailUrl);
        Task<List<WorkflowEmailViewModel>> CompletedDedocketInstructionWorkflow(IList<ActionDueViewModel> updated, string? emailUrl);
        Task<List<WorkflowEmailViewModel>> DeletedActionDueWorkflow(PatActionDue actionDue, string? emailUrl, string? delegatedEmailUrl, List<LookupIntDTO> openDelegatedDdIds);
        Task<List<WorkflowEmailViewModel>> NewDelegatedTaskWorkflow(string? emailUrl, List<PatDueDateDelegationDetail> newDelegations);
        Task<List<WorkflowEmailViewModel>> CompletedDelegatedTaskWorkflow(int actId, string emailUrl, List<LookupIntDTO> dddIds);
        Task<List<WorkflowEmailViewModel>> ReassignedDelegatedTaskWorkflow(string? emailUrl, List<PatDueDateDelegationDetail> deletedDelegations);
        Task<List<WorkflowEmailViewModel>> DuedateChangedDelegatedTaskWorkflow(int actId, string? emailUrl, List<LookupIntDTO> dddIds);
        Task<List<WorkflowEmailViewModel>> DeletedDelegatedTaskWorkflow(int actId, string? emailUrl, List<DelegationEmailDTO> emails);
        Task<List<WorkflowEmailViewModel>> NewRequestDocketWorkflow(int appId, int reqId, string? emailUrl);
        #endregion
    }
}
