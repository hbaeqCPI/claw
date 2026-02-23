using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class ProductViewModelService : IProductViewModelService
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public ProductViewModelService(IProductService productService, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _productService = productService;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<Product> AddCriteria(IQueryable<Product> products, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var productCodeInitial = mainSearchFilters.FirstOrDefault(f => f.Property == "ProductCodeInitial");
                if (productCodeInitial != null)
                {
                    products = products.Where(i => !string.IsNullOrEmpty(i.ProductCode) && i.ProductCode.StartsWith(productCodeInitial.Value));
                    mainSearchFilters.Remove(productCodeInitial);
                }

                if (mainSearchFilters.Any())
                    products = QueryHelper.BuildCriteria(products, mainSearchFilters);
            }
            return products;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Product> products)
        {
            var model = products.ProjectTo<ProductSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(product => product.ProductCode);

            var ids = await model.Select(c => c.ProductId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<ProductDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var product = await _productService.QueryableList.Where(a => a.ProductId == id).ProjectTo<ProductDetailViewModel>().FirstOrDefaultAsync();
            return product;
        }

        public async Task<int?> GetProductId(string productCode)
        {
            var product = await _productService.QueryableList.Where(a => a.ProductCode == productCode).FirstOrDefaultAsync();
            return product?.ProductId;
        }

    }
}
