using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.Entities;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IProductViewModelService
    {
        IQueryable<Product> AddCriteria(IQueryable<Product> products, List<QueryFilterViewModel> mainSearchFilters);
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Product> products);
        Task<ProductDetailViewModel> CreateViewModelForDetailScreen(int id);
        Task<int?> GetProductId(string productCode);

    }
}

 