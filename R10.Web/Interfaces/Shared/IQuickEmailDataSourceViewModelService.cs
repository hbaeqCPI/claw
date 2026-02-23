using Kendo.Mvc.UI;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IQEDataSourceViewModelService
    {
        IQueryable<QEDataSource> AddCriteria(IQueryable<QEDataSource> dataSources, List<QueryFilterViewModel> mainSearchFilters);
        Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<QEDataSource> dataSources);
        Task<QEDataSourceDetailViewModel> CreateViewModelForDetailScreen(int id);

    }
}
