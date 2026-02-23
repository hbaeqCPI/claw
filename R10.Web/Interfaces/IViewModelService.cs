using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IViewModelService<T>
    {
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<T> list, string defaultSortOrder, string idProperty);
        Task<CPiDataSourceResult> CreateViewModelForGrid<T2>(DataSourceRequest request, IQueryable<T> list, string defaultSortOrder, string idProperty);
        IQueryable<T> AddCriteria(List<QueryFilterViewModel> mainSearchFilters);
        IQueryable<T> AddCriteria(IQueryable<T> list, List<QueryFilterViewModel> mainSearchFilters);
        Task<T> GetEntityByCode(string property, string value);
        T2 MapToDomainModel<T2, T3>(T3 vm);
    }
}

 