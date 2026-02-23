using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities.Clearance;
using R10.Web.Areas.Clearance.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core;

namespace R10.Web.Interfaces
{
    public interface ITmcClearanceViewModelService
    {
        Task<TmcClearanceDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmcClearance> clearances);
        IQueryable<TmcClearance> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmcClearance> clearances);
        TmcClearance ConvertViewModelToTmcClearance(TmcClearanceDetailViewModel viewModel);
        IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<TmcClearance> clearances,
            DataSourceRequest request, string textProperty, string text, FilterType filterType);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmcClearance> clearances, string value);        

        Task<List<TmcQuestionTabViewModel>> GetQuestionTabList(int tmcId);

        Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(string? emailUrl, string? userName, TmcClearance clearance, bool checkStatusChangeWorkFlow, string? oldClearanceStatus, bool checkNewDiscussionWorkflow = false, bool checkDiscussionReplyWorkflow = false);
    }
}