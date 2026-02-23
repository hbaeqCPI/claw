using Kendo.Mvc.UI;
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
    public interface IPatCostTrackingViewModelService
    {

        Task<PatCostTrackingDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatCostTrack> costTracks);
        IQueryable<PatCostTrack> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<PatCostTrack> costTracks);
        PatCostTrack ConvertViewModelToCostTracking(PatCostTrackingDetailViewModel viewModel);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<PatCostTrack> costTracks, string value);

        Task<List<PatCostTrackingAppInfoViewModel>> GetAppInfoList(string caseNumber, string country, string subCase);

        IQueryable<PatCostTrackingAppInfoViewModel> AppInfo { get; }
    }
}
