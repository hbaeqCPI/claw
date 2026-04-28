using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Patent;
using LawPortal.Web.Areas;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Extensions;

namespace LawPortal.Web.Interfaces
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

 