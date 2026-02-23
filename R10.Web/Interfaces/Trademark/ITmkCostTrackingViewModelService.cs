using Kendo.Mvc.UI;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ITmkCostTrackingViewModelService
    {

        Task<TmkCostTrackingDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkCostTrack> costTracks);
        IQueryable<TmkCostTrack> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkCostTrack> costTracks);
        TmkCostTrack ConvertViewModelToCostTracking(TmkCostTrackingDetailViewModel viewModel);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkCostTrack> costTracks, string value);

        Task<List<TmkCostTrackingTmkInfoViewModel>> GetTmkInfoList(string caseNumber, string country, string subCase);

        IQueryable<TmkCostTrackingTmkInfoViewModel> TmkInfo { get; }
    }
}
