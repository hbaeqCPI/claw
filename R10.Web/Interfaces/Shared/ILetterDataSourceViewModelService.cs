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
    public interface ILetterDataSourceViewModelService
    {
        IQueryable<LetterDataSource> AddCriteria(IQueryable<LetterDataSource> dataSources, List<QueryFilterViewModel> mainSearchFilters);
        Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<LetterDataSource> dataSources);
        Task<LetterDataSourceDetailViewModel> CreateViewModelForDetailScreen(int id);

    }
}
