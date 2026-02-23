using Kendo.Mvc.UI;
using R10.Core.Entities.ReportScheduler;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces.Shared
{
    public interface IRSMainViewModelService
    {
        Task<RSMainDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<RSMain> rSMains);
        IQueryable<RSMain> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<RSMain> rSMains);
        RSMain ConvertViewModelToRSMain(RSMainDetailViewModel viewModel);
        IQueryable<ScheduleNameLookupViewModel> GetScheduleNamesList(IQueryable<RSMain> rSMains,
            DataSourceRequest request, string textProperty, string text, FilterType filterType);

        Task<ScheduleNameLookupViewModel> ScheduleNameSearchValueMapper(IQueryable<RSMain> rSMains, string value);
    }
}
