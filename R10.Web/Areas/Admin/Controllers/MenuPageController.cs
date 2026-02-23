using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Areas.Admin.Views;
using R10.Core.Entities;
using R10.Web.Models.PageViewModels;
using R10.Web.Extensions;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Web.Extensions.ActionResults;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Admin.Helpers;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class MenuPageController : BaseController
    {
        private readonly ICPiMenuPageManager _pageManager;
        private readonly IStringLocalizer<AdminResource> _localizer;

        public MenuPageController(ICPiMenuPageManager pageManager, IStringLocalizer<AdminResource> localizer)
        {
            _pageManager = pageManager;
            _localizer = localizer;
        }
        private string DataContainer => "menuPageDetail";
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            var model = new PageViewModel()
            {
                PageId = "menuPageSearch",
                Title = _localizer["Menu Pages"].ToString(),
                CanAddRecord = true 
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.MenuPages
            };

            if (Request.IsAjax())
                return PartialView("Index", sidebarModel);

            return View(sidebarModel);
        }

        [HttpGet]
        public IActionResult Search(string status)
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var pages = _pageManager.MenuPages.AddCriteria(mainSearchFilters).ProjectTo<MenuPageListViewModel>();

                if (request.Sorts != null && request.Sorts.Any())
                    pages = pages.ApplySorting(request.Sorts);
                else
                    pages = pages.OrderBy(p => p.Name);

                var ids = await pages.Select(p => p.Id).ToArrayAsync();

                return Json(new CPiDataSourceResult()
                {
                    Data = await pages.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<MenuPageDetailViewModel>> PrepareEditScreen(int id)
        {
            var detail = await _pageManager.MenuPages.Where(p => p.Id == id).ProjectTo<MenuPageDetailViewModel>().SingleOrDefaultAsync();
            var viewModel = new DetailPageViewModel<MenuPageDetailViewModel> { Detail = detail };

            if (detail != null)
            {
                viewModel.CanAddRecord = true;
                viewModel.CanEditRecord = true;
                viewModel.CanDeleteRecord = true;
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = DataContainer;

                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
            }

            return viewModel;
        }

        private DetailPageViewModel<MenuPageDetailViewModel> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<MenuPageDetailViewModel>
            {
                Detail = new MenuPageDetailViewModel() { Policy = "*" }
            };

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = DataContainer;
            return viewModel;
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
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
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Page Detail"].ToString(),
                RecordId = detail.Id,
                SingleRecord = singleRecord || !Request.IsAjax(),
                PagePermission = page,
                Data = detail
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.MenuPages
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                {
                    model.Page = PageType.DetailContent;
                    return PartialView("_Index", model);
                }

                return PartialView("Index", sidebarModel);
            }

            return View("Index", sidebarModel);
        }

        public IActionResult Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["New Page"].ToString(),
                RecordId = detail.Id,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.MenuPages
            };

            if (!fromSearch)
            {
                model.Page = PageType.DetailContent;
                return PartialView("_Index", model);
            }

            return PartialView("Index", sidebarModel);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] CPiMenuPage menuPage)
        {
            if (ModelState.IsValid)
            {
                await _pageManager.SaveMenuPage(menuPage);
                return Json(menuPage.Id);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var menuPage = await _pageManager.GetMenuPageByIdAsync(id);
            if (menuPage == null)
                return BadRequest(_localizer["Menu page not found."].ToString());

            await _pageManager.RemoveMenuPage(menuPage);
            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(_pageManager.MenuPages.ProjectTo<MenuPageListViewModel>(), request, property, text, filterType, requiredRelation);
            return result;
        }

    }
}