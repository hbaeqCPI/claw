using Kendo.Mvc.UI;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ITmkConflictViewModelService
    {
        Task<TmkConflictDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkConflict> conflicts);
        IQueryable<TmkConflict> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkConflict> conflicts);
        TmkConflict ConvertViewModelToConflict(TmkConflictDetailViewModel viewModel);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkConflict> costTracks, string value);
        Task<List<TmkConflictTmkInfoViewModel>> GetTmkInfoList(string caseNumber, string country, string subCase);

        IQueryable<TmkConflictTmkInfoViewModel> TmkInfo { get; }
    }
}
