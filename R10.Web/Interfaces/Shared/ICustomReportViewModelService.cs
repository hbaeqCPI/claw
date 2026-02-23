using Kendo.Mvc.UI;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces.Shared
{
    public interface ICustomReportViewModelService
    {
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<CustomReport> customReports);
        Task<CustomReportDetailViewModel> CreateViewModelForDetailScreen(string ReportName);

        IQueryable<CustomReport> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<CustomReport> customReports, string userId);
    }
}
