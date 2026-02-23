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
    public interface IPatCostTrackingInvViewModelService
    {

        Task<PatCostTrackingInvDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatCostTrackInv> costTracks);
        IQueryable<PatCostTrackInv> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<PatCostTrackInv> costTracks);
        PatCostTrackInv ConvertViewModelToCostTracking(PatCostTrackingInvDetailViewModel viewModel);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<PatCostTrackInv> costTracks, string value);

        Task<List<PatCostTrackingInvInvInfoViewModel>> GetInvInfoList(string caseNumber);

        IQueryable<PatCostTrackingInvInvInfoViewModel> InvInfo { get; }
    }
}