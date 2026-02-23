using Kendo.Mvc.UI;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Extensions;
using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ITmkActionDueViewModelService
    {
        Task<TmkActionDueDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkActionDue> actionsDue, List<QueryFilterViewModel> dueDateFilters);
        IQueryable<TmkActionDue> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkActionDue> actionsDue);
        TmkActionDue ConvertViewModelToActionDue(TmkActionDueDetailViewModel viewModel);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkActionDue> actionsDue, string value);

        Task<List<TmkActionDueTmkInfoViewModel>> GetTmkInfoList(string caseNumber, string country, string subCase);

        IQueryable<TmkActionDueTmkInfoViewModel> TmkInfo { get; }

        #region Workflow
        Task<List<WorkflowEmailViewModel>> NewOrCompletedActionWorkflow(TmkActionDue actionDue, string? emailUrl, bool newAction);
        Task<List<WorkflowEmailViewModel>> NewDedocketInstructionWorkflow(IList<ActionDueViewModel> updated, string? emailUrl);
        Task<List<WorkflowEmailViewModel>> CompletedDedocketInstructionWorkflow(IList<ActionDueViewModel> updated, string? emailUrl);
        Task<List<WorkflowEmailViewModel>> DeletedActionDueWorkflow(TmkActionDue actionDue, string? emailUrl, string? delegatedEmailUrl, List<LookupIntDTO> openDelegatedDdIds);
        Task<List<WorkflowEmailViewModel>> NewDelegatedTaskWorkflow(string? emailUrl, List<TmkDueDateDelegationDetail> newDelegations);
        Task<List<WorkflowEmailViewModel>> CompletedDelegatedTaskWorkflow(int actId, string emailUrl, List<LookupIntDTO> dddIds);
        Task<List<WorkflowEmailViewModel>> ReassignedDelegatedTaskWorkflow(string? emailUrl, List<TmkDueDateDelegationDetail> deletedDelegations);
        Task<List<WorkflowEmailViewModel>> DuedateChangedDelegatedTaskWorkflow(int actId, string? emailUrl, List<LookupIntDTO> dddIds);
        Task<List<WorkflowEmailViewModel>> DeletedDelegatedTaskWorkflow(int actId, string? emailUrl, List<DelegationEmailDTO> emails);
        Task<List<WorkflowEmailViewModel>> NewRequestDocketWorkflow(int tmkId, int reqId, string? emailUrl);

        #endregion
    }
}
