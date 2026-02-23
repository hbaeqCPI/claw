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
    public interface IInventionViewModelService
    {
        Task<InventionDetailViewModel> CreateViewModelForDetailScreen(int id);

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Invention> inventions);
        IQueryable<Invention> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<Invention> inventions);
        Invention ConvertViewModelToInvention(InventionDetailViewModel viewModel);
        IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<Invention> inventions,
            DataSourceRequest request, string textProperty, string text, FilterType filterType);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<Invention> inventions, string value);
        Task<TitleLookupViewModel> TitleSearchValueMapper(IQueryable<Invention> inventions, string value);

        Task ApplyDetailPageTradeSecretPermission(DetailPageViewModel<InventionDetailViewModel> viewModel);
    }
}
