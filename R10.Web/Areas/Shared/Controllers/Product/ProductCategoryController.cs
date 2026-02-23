using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ProductCategoryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IEntityService<ProductCategory> _auxService;
        private readonly IViewModelService<ProductCategory> _viewModelService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IReportService _reportService;

        private readonly string _dataContainer = "productCategoryDetail";

        public ProductCategoryController(
            IAuthorizationService authService,
            IEntityService<ProductCategory> auxService,
            IViewModelService<ProductCategory> viewModelService,
            IReportService reportService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _auxService = auxService;
            _viewModelService = viewModelService;
            _reportService = reportService;
            _localizer = localizer;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> Patent()
        {
            return await Index(SystemTypeCode.Patent);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> Trademark()
        {
            return await Index(SystemTypeCode.Trademark);
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> GeneralMatter()
        {
            return await Index(SystemTypeCode.GeneralMatter);
        }

        [Authorize(Policy = AMSAuthorizationPolicy.CanAccessProducts)]
        public async Task<IActionResult> AMS()
        {
            return await Index(SystemTypeCode.AMS);
        }

        public async Task<IActionResult> Index(string systemType = SystemTypeCode.Patent)
        {

            var authorized = false;

            if (systemType == SystemTypeCode.Trademark)
                authorized = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessProducts)).Succeeded;
            else if (systemType == SystemTypeCode.GeneralMatter)
                authorized = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessProducts)).Succeeded;
            else if (systemType == SystemTypeCode.AMS)
                authorized = (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CanAccessProducts)).Succeeded;
            else
                authorized = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessProducts)).Succeeded;

            if (!authorized)
                return Forbid();

            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "productCategorySearch",
                Title = _localizer["Product Category Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "productCategorySearchResults",
                Title = _localizer["Product Category Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var productCategory = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, productCategory, "ProductCategory", "ProductCategoryId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("ProductCategoryName", id);
                if (entity == null)
                    return RedirectToAction(nameof(Add), new { productCategory = id, fromSearch = true });
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.ProductCategoryId, singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Product Category Detail"].ToString(),
                RecordId = detail.ProductCategoryId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "ProductCategory Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel sharedProductCategoryPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(sharedProductCategoryPrintModel, ReportType.SharedProductCategoryPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(string productCategory = "", bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            page.Detail.ProductCategoryName = productCategory;
            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Product Category"].ToString(),
                RecordId = detail.ProductCategoryId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var entity = await _auxService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _auxService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(productCategory, productCategory.ProductCategoryId);

                if (productCategory.ProductCategoryId > 0)
                    await _auxService.Update(productCategory);
                else
                    await _auxService.Add(productCategory);

                return Json(productCategory.ProductCategoryId);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var productCategory = await _auxService.GetByIdAsync(id);
            if (productCategory == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { productCategory.CreatedBy, productCategory.DateCreated, productCategory.UpdatedBy, productCategory.LastUpdate, productCategory.tStamp });
        }

        private async Task<DetailPageViewModel<ProductCategory>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<ProductCategory>();
            viewModel.Detail = await _auxService.GetByIdAsync(id);

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<ProductCategory>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<ProductCategory>();
            viewModel.Detail = new ProductCategory();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetProductCategoryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, new string[] { "ProductCategoryName", "Description" }, requiredRelation);
        }
    }
}