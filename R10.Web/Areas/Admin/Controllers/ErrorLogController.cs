using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.Views;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class ErrorLogController : BaseController
    {
        private readonly ILoggerService<Log> _errorLogger;
        private readonly IStringLocalizer<AdminResource> _localizer;

        private readonly string _dataContainer = "errorLog";

        public ErrorLogController(ILoggerService<Log> errorLogger, IStringLocalizer<AdminResource> localizer)
        {
            _errorLogger = errorLogger;
            _localizer = localizer;
        }

        protected IQueryable<Log> Logs => _errorLogger.QueryableList;
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            var model = new PageViewModel()
            {
                //Page = PageType.Search,
                PageId = "errorlogSearch",
                Title = _localizer["Error Logs"].ToString(),
                CanAddRecord = false,
                HasSavedCriteria = true
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                //MainPartialView = "_SidebarSearchPage", //search page is not used
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Logs
            };

            if (Request.IsAjax())
                return PartialView("Index", sidebarModel);

            return View(sidebarModel);
        }

        [HttpGet()]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var logs = mainSearchFilters.Any() ? QueryHelper.BuildCriteria<Log>(Logs, mainSearchFilters) : Logs;

                if (request.Sorts != null && request.Sorts.Any())
                    logs = logs.ApplySorting(request.Sorts);
                else
                    logs = logs.OrderByDescending(p => p.TimeStamp);

                var ids = await logs.Select(p => p.Id).ToArrayAsync();

                return Json(new CPiDataSourceResult()
                {
                    Data = await logs.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<Log>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<Log>
            {
                Detail = await _errorLogger.GetByIdAsync(id)
            };

            if (viewModel.Detail != null)
            {
                //read-only. no security policies needed

                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;
                viewModel.CanAddRecord = false;
                viewModel.CanPrintRecord = false;

                AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = Url.Action("Index");
            }
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
                Title = _localizer["Error Log Detail"].ToString(),
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
                SideBarViewModel = AdminNavPages.Logs
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

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(Logs, request, property, text, filterType, requiredRelation);
        }
    }
}