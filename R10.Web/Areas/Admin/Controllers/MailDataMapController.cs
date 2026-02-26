using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.MailDownload;
using R10.Core.Helpers;
// using R10.Core.Interfaces.AMS; // Removed during deep clean
using R10.Web.Areas.Admin.Helpers;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Admin.Views;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services.MailDownload;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class MailDataMapController : BaseController
    {
        private readonly IMailDataMapService _dataMapService;
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<MailDataMapController> _localizer;

        private string DataContainer => "dataMapDetail";
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public MailDataMapController(IMailDataMapService dataMapService, IAuthorizationService authService, IStringLocalizer<MailDataMapController> localizer)
        {
            _dataMapService = dataMapService;
            _authService = authService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                PageId = "dataMapSearch",
                Title = _localizer["Mail Data Patterns"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, CPiAuthorizationPolicy.CPiAdmin)).Succeeded
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.MailDataMap
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
                var pages = _dataMapService.Maps.AddCriteria(mainSearchFilters).ProjectTo<MailDataMapListViewModel>();

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

        private async Task<DetailPageViewModel<MailDataMapDetailViewModel>> PrepareEditScreen(int id)
        {
            var detail = await _dataMapService.Maps.Where(m => m.Id == id).ProjectTo<MailDataMapDetailViewModel>().SingleOrDefaultAsync();
            var viewModel = new DetailPageViewModel<MailDataMapDetailViewModel> { Detail = detail };

            if (detail != null)
            {
                var isCPiAdmin = (await _authService.AuthorizeAsync(User, CPiAuthorizationPolicy.CPiAdmin)).Succeeded;

                viewModel.CanAddRecord = isCPiAdmin;
                viewModel.CanDeleteRecord = isCPiAdmin;
                viewModel.CanEditRecord = isCPiAdmin;
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = DataContainer;

                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
            }

            return viewModel;
        }

        private DetailPageViewModel<MailDataMapDetailViewModel> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<MailDataMapDetailViewModel>
            {
                Detail = new MailDataMapDetailViewModel()
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
                Title = _localizer["Map Detail"].ToString(),
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
                SideBarViewModel = AdminNavPages.MailDataMap
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

        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
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
                Title = _localizer["New Map"].ToString(),
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
                SideBarViewModel = AdminNavPages.MailDataMap
            };

            if (!fromSearch)
            {
                model.Page = PageType.DetailContent;
                return PartialView("_Index", model);
            }

            return PartialView("Index", sidebarModel);
        }

        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] MailDownloadDataMap map)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(map, map.Id);
                await _dataMapService.SaveMap(map);
                return Json(map.Id);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var map = await _dataMapService.Maps.Where(m => m.Id == id).FirstOrDefaultAsync();
            if (map == null)
                return BadRequest(_localizer["Map not found."].ToString());

            await _dataMapService.RemoveMap(map);
            return Ok();
        }

        public async Task<IActionResult> MapPatternsRead([DataSourceRequest] DataSourceRequest request, int id)
        {
            var result = await _dataMapService.MapPatterns
                                            .Where(p => p.MapId == id)
                                            .OrderBy(p => p.Pattern)
                                            .ToListAsync();

            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> MapPatternsUpdate(int id,
            [Bind(Prefix = "updated")] IEnumerable<MailDownloadDataMapPattern> updated,
            [Bind(Prefix = "new")] IEnumerable<MailDownloadDataMapPattern> added,
            [Bind(Prefix = "deleted")] IEnumerable<MailDownloadDataMapPattern> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _dataMapService.UpdateMapPatterns(id, User.GetUserName(), updated, added, deleted);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Pattern has been saved successfully."].ToString() :
                _localizer["Patterns have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public async Task<IActionResult> MapPatternsDelete([Bind(Prefix = "deleted")] MailDownloadDataMapPattern deleted)
        {
            if (deleted.Id > 0)
            {
                await _dataMapService.UpdateMapPatterns(deleted.MapId, User.GetUserName(), new List<MailDownloadDataMapPattern>(), new List<MailDownloadDataMapPattern>(), new List<MailDownloadDataMapPattern>() { deleted });
                return Ok(new { success = _localizer["Pattern has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var map = await _dataMapService.Maps.Where(m => m.Id == id).FirstOrDefaultAsync();
            if (map == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = map.CreatedBy, dateCreated = map.DateCreated, updatedBy = map.UpdatedBy, lastUpdate = map.LastUpdate, tStamp = map.tStamp });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(_dataMapService.Maps.ProjectTo<MailDataMapListViewModel>(), request, property, text, filterType, requiredRelation);
            return result;
        }

        public async Task<IActionResult> GetPatterns([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(_dataMapService.MapPatterns, request, property, text, filterType, requiredRelation);
            return result;
        }

        public async Task<IActionResult> GetMaps()
        {
            var maps = await _dataMapService.Maps.Select(m => new { Id = m.Id, Name = m.Name }).ToListAsync();
            return Json(maps);
        }

        public async Task<IActionResult> GetAttributes()
        {
            var maps = await _dataMapService.Attributes.Select(m => new { Id = m.Id, Name = m.Name }).ToListAsync();
            return Json(maps);
        }
    }
}
