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
    public interface IDataQueryViewModelService
    {
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<DataQueryMain> dataQueriesMain);
        Task<DataQueryDetailViewModel> CreateViewModelForDetailScreen(int id);

        IQueryable<DataQueryMain> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<DataQueryMain> dataQueriesMain, string userName, bool isAdmin = false);

        string GetSQLExpr(int id);

    }
}
