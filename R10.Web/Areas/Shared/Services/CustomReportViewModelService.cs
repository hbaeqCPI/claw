using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class CustomReportViewModelService : ICustomReportViewModelService
    {
        private readonly IEntityService<CustomReport> _customReportService;
        private readonly IDataQueryService _dataQueryService;

        public CustomReportViewModelService(IEntityService<CustomReport> customReportService, IDataQueryService dataQueryService)
        {
            _customReportService = customReportService;
            _dataQueryService = dataQueryService;
        }

        public IQueryable<CustomReport> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<CustomReport> customReports, string userId)
        {
            customReports = customReports.Where(q => q.IsShared || q.UserId == userId);
            if (mainSearchFilters.Count > 0)
            {
                var showOnlyMyReportsFilter = mainSearchFilters.FirstOrDefault(f => f.Property == "ShowOnlyMyReports");
                var showOnlyMyReports = showOnlyMyReportsFilter != null;
                if (showOnlyMyReports)
                {
                    customReports = customReports.Where(q => q.UserId == userId);
                    mainSearchFilters.Remove(showOnlyMyReportsFilter);
                }
                if (mainSearchFilters.Any())
                    customReports = QueryHelper.BuildCriteria<CustomReport>(customReports, mainSearchFilters);
            }
            return customReports;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<CustomReport> customReports)
        {
            var model = customReports.ProjectTo<CustomReportSearchResultViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(q => q.ReportName);

            //var ids = await model.Select(q => q.).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = customReports.Count(),
            };

        }

        public async Task<CustomReportDetailViewModel> CreateViewModelForDetailScreen(string reportName)
        {
            var viewModel = new CustomReportDetailViewModel() { 
                IsEditable = true,
                IsShared = true
            };

            if (reportName != null)
            {
                viewModel = await _customReportService.QueryableList.ProjectTo<CustomReportDetailViewModel>()
                                .SingleOrDefaultAsync(q => q.ReportName == reportName);
                var query = await _dataQueryService.GetByIdAsync(viewModel!.QueryId ?? 0);
                if (query != null)
                    viewModel.ReportDataSource = query.QueryName;
            }

            return viewModel;
        }
    }
}
