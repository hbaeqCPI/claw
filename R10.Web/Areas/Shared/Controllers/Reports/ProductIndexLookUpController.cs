using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.AMS; // Removed during deep clean
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using R10.Core.Interfaces.Patent;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Controllers.Reports
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ProductIndexLookUpController : SharedReportBaseLookUpController
    {
        private readonly IProductService _productService;

        public ProductIndexLookUpController(IInventionService inventionService
            , ICountryApplicationService applicationService
            , ISharedReportViewModelService sharedReportViewModelService
            , ITmkTrademarkService trademarkService
//             , IGMMatterService gmMatterService // Removed during deep clean

//             , IDisclosureService disclosureService // Removed during deep clean
//             , IAMSDueService amsDueService // Removed during deep clean
            , ISystemSettings<PatSetting> patSettings
            , IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService
            , IMultipleEntityService<PatOwnerApp> patOwnerAppService
            , IEntityService<TmkOwner> tmkOwnerService
            // Removed during deep clean - GM module removed
            // , IMultipleEntityService<GMMatter, GMMatterAttorney> matterAttorneyService
            // , IGMMatterCountryService matterCountryService

            , IDueDateService<PatActionDue, PatDueDate> patDueDateService
            , IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService
            // Removed during deep clean - GM module removed
            // , IDueDateService<GMActionDue, GMDueDate> gmDueDateService
            // Removed during deep clean - DMS module removed
            // , IDueDateService<DMSActionDue, DMSDueDate> dmsDueDateService
            , IProductService productService
            ) : base(inventionService, applicationService, sharedReportViewModelService, trademarkService, patSettings, patOwnerInvService, patOwnerAppService, tmkOwnerService
            , patDueDateService, tmkDueDateService)
        {
            _productService = productService;
            CountryApplications = applicationService.CountryApplications;
            TmkTrademarks = trademarkService.TmkTrademarks;
            // Removed during deep clean - GM module removed
            // GMMatters = gmMatterService.QueryableList;
            Inventions = inventionService.QueryableList;
        }

        public async Task<IActionResult> GetTitleList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            if (systemType == null||systemType=="") systemType = "P,T";

            var appTitles = systemType.Contains("P") ? CountryApplications.Where(c => c.Products != null && c.Products.Any() && c.AppTitle != null).Select(c => new SharedEntity { Code = c.AppTitle }).Distinct().ToList() : new List<SharedEntity>();
            var invTitles = systemType.Contains("P") ? Inventions.Where(c => c.Products != null && c.Products.Any() && c.InvTitle != null).Select(c => new SharedEntity { Code = c.InvTitle }).Distinct().ToList() : new List<SharedEntity>();
            var tmkTitles = systemType.Contains("T") ? TmkTrademarks.Where(c => c.TmkProducts != null && c.TmkProducts.Any() && c.TrademarkName != null).Select(c => new SharedEntity { Code = c.TrademarkName }).Distinct().ToList() : new List<SharedEntity>();
            // Removed during deep clean - GM module removed
            // var gmTitles = systemType.Contains("G") ? GMMatters.Where(c => c.GMProducts != null && c.GMProducts.Any() && c.MatterTitle != null).Select(c => new SharedEntity { Code = c.MatterTitle }).Distinct().ToList() : new List<SharedEntity>();

            var result2 = new List<SharedEntity>();
            if (systemType.Contains("P"))
            {
                result2.AddRange(appTitles);
                result2.AddRange(invTitles);
            }
            if (systemType.Contains("T"))
                result2.AddRange(tmkTitles);
            // Removed during deep clean - GM module removed
            // if (systemType.Contains("G"))
            //     result2.AddRange(gmTitles);

            var result = result2.Where(c => c.Code.StartsWith(text == null ? "" : text)).Select(c=> new { Title = c.Code }).OrderBy(c => c.Title);

            if (request.PageSize > 0)
            {
                request.Filters.Clear();
                return Json(await result.ToDataSourceResultAsync(request));
            }

            var list = result.ToList();
            return Json(list);
        }

        public async Task<IActionResult> GetProductList(string property, string text, FilterType filterType)
        {
            var products = _productService.QueryableList.Where(c => _applicationService.QueryableChildList<PatProduct>().Any(p => CountryApplications.Any(ca => ca.AppId == p.AppId) && p.ProductId == c.ProductId))
                        .Union(_productService.QueryableList.Where(c => _trademarkService.QueryableChildList<TmkProduct>().Any(p => TmkTrademarks.Any(t => t.TmkId == p.TmkId) && p.ProductId == c.ProductId)));
                        // Removed during deep clean - GM module removed
                        // .Union(_productService.QueryableList.Where(c => _gmMatterService.QueryableChildList<GMProduct>().Any(p => GMMatters.Any(gm => gm.MatId == p.MatId) && p.ProductId == c.ProductId)));

            return Json(await products.Select(p => new { ProductCode = p.ProductCode, Product = p.ProductName }).Distinct().OrderBy(p => p.Product).ToListAsync());
        }
        public async Task<IActionResult> GetProductCategoryList(string property, string text, FilterType filterType)
        {
            var products = _productService.QueryableList.Where(c => _applicationService.QueryableChildList<PatProduct>().Any(p => CountryApplications.Any(ca => ca.AppId == p.AppId) && p.ProductId == c.ProductId))
                 .Union(_productService.QueryableList.Where(c => _trademarkService.QueryableChildList<TmkProduct>().Any(p => TmkTrademarks.Any(t => t.TmkId == p.TmkId) && p.ProductId == c.ProductId)));
                 // Removed during deep clean - GM module removed
                 // .Union(_productService.QueryableList.Where(c => _gmMatterService.QueryableChildList<GMProduct>().Any(p => GMMatters.Any(gm => gm.MatId == p.MatId) && p.ProductId == c.ProductId)));

            return Json(await products.Select(p => new { ProductCategory = p.ProductCategory }).Where(p => p.ProductCategory != null).Distinct().OrderBy(p => p.ProductCategory).ToListAsync());
        }
        public async Task<IActionResult> GetProductBrandList(string property, string text, FilterType filterType)
        {
            var products = _productService.QueryableList.Where(c => _applicationService.QueryableChildList<PatProduct>().Any(p => CountryApplications.Any(ca => ca.AppId == p.AppId) && p.ProductId == c.ProductId))
                             .Union(_productService.QueryableList.Where(c => _trademarkService.QueryableChildList<TmkProduct>().Any(p => TmkTrademarks.Any(t => t.TmkId == p.TmkId) && p.ProductId == c.ProductId)));
                             // Removed during deep clean - GM module removed
                             // .Union(_productService.QueryableList.Where(c => _gmMatterService.QueryableChildList<GMProduct>().Any(p => GMMatters.Any(gm => gm.MatId == p.MatId) && p.ProductId == c.ProductId)));

            return Json(await products.Select(p => new { Brand = p.Brand }).Where(p => p.Brand != null).Distinct().OrderBy(p => p.Brand).ToListAsync());
        }
        public async Task<IActionResult> GetProductGroupList(string property, string text, FilterType filterType)
        {
            var products = _productService.QueryableList.Where(c => _applicationService.QueryableChildList<PatProduct>().Any(p => CountryApplications.Any(ca => ca.AppId == p.AppId) && p.ProductId == c.ProductId))
                             .Union(_productService.QueryableList.Where(c => _trademarkService.QueryableChildList<TmkProduct>().Any(p => TmkTrademarks.Any(t => t.TmkId == p.TmkId) && p.ProductId == c.ProductId)));
                             // Removed during deep clean - GM module removed
                             // .Union(_productService.QueryableList.Where(c => _gmMatterService.QueryableChildList<GMProduct>().Any(p => GMMatters.Any(gm => gm.MatId == p.MatId) && p.ProductId == c.ProductId)));

            return Json(await products.Select(p => new { ProductGroup = p.ProductGroup }).Where(p => p.ProductGroup != null).Distinct().OrderBy(p => p.ProductGroup).ToListAsync());
        }
    }
}
