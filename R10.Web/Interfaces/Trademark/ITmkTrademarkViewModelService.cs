using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core;

namespace R10.Web.Interfaces
{
    public interface ITmkTrademarkViewModelService
    {
        Task<TmkTrademarkDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkTrademark> trademarks);
        IQueryable<TmkTrademark> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkTrademark> trademarks );
        TmkTrademark ConvertViewModelToTmkTrademark(TmkTrademarkDetailViewModel viewModel);
        IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<TmkTrademark> trademarks,
            DataSourceRequest request, string textProperty, string text, FilterType filterType);
        Task<FamilyTreeDiagram> GetFamilyTreeDiagram(string paramType, string paramValue);
        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkTrademark> trademarks, string value);
        Task<TrademarkNameLookupViewModel> TrademarkNameValueMapper(IQueryable<TmkTrademark> trademarks, string value);
        Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(TmkTrademark trademark, bool checkStatusChangeWorkFlow, bool checkAttyChangeWorkFlow, string? oldTrademarkStatus,
                                              string? emailUrl, string? attyEmailUrl, string? userName, int delegationId,string? delegationEmailUrl, string? actionEmailUrl);
    }
}
