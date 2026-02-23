using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Web.Models;
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
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Core.Helpers;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class LetterSubCategoryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IEntityService<LetterSubCategory> _auxService;
        private readonly IViewModelService<LetterSubCategory> _viewModelService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IReportService _reportService;

        private readonly string _dataContainer = "letterSubCategoryDetail";

        public LetterSubCategoryController(
            IAuthorizationService authService,
            IEntityService<LetterSubCategory> auxService,
            IViewModelService<LetterSubCategory> viewModelService,
            IReportService reportService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _auxService = auxService;
            _viewModelService = viewModelService;
            _reportService = reportService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "letterSubCategorySearch",
                Title = _localizer["Letter Sub Category Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "letterSubCategorySearchResults",
                Title = _localizer["Letter Sub Category Search Results"].ToString(),
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
                var letterSubCategorys = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, letterSubCategorys, "LetSubCat", "letSubCatId");
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
                var entity = await _viewModelService.GetEntityByCode("LetSubCat", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.LetSubCatId, singleRecord = true, fromSearch = true });
            }
            else
                return new RecordDoesNotExistResult();
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
                Title = _localizer["Letter Sub Category Detail"].ToString(),
                RecordId = detail.LetSubCatId,
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
            ViewBag.DownloadName = "Letter Sub Category Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel letterSubCategoryPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(letterSubCategoryPrintModel, ReportType.LetterSubCategoryPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Sub Category"].ToString(),
                RecordId = detail.LetSubCatId,
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
        public async Task<IActionResult> Save([FromBody] LetterSubCategory letterSubCategory)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(letterSubCategory, letterSubCategory.LetSubCatId);

                if (letterSubCategory.LetSubCatId > 0)
                    await _auxService.Update(letterSubCategory);
                else
                    await _auxService.Add(letterSubCategory);

                return Json(letterSubCategory.LetSubCatId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var letterSubCategory = await _auxService.GetByIdAsync(id);
            if (letterSubCategory == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = letterSubCategory.CreatedBy, dateCreated = letterSubCategory.DateCreated, updatedBy = letterSubCategory.UpdatedBy, lastUpdate = letterSubCategory.LastUpdate, tStamp = letterSubCategory.tStamp });
        }

        public async Task<IActionResult> GetPicklistData(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var letterSubCategorys = _auxService.QueryableList;
            var result = await QueryHelper.GetPicklistDataAsync(letterSubCategorys, property, text, filterType, requiredRelation);
            return Json(result);
        }

        public async Task<IActionResult> GetLetterSubCategoryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList.Select(s => new { LetSubCatId = s.LetSubCatId, LetSubCat = s.LetSubCat }), request, property, text, filterType, requiredRelation, false);
        }

        private async Task<DetailPageViewModel<LetterSubCategory>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<LetterSubCategory>();
            viewModel.Detail = await _auxService.QueryableList.SingleOrDefaultAsync(c => c.LetSubCatId == id);

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

        private async Task<DetailPageViewModel<LetterSubCategory>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<LetterSubCategory>();
            viewModel.Detail = new LetterSubCategory();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

    }
}