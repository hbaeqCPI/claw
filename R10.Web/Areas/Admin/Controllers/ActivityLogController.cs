using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Areas.Admin.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class ActivityLogController : BaseController
    {
        private readonly ILoggerService<ActivityLog> _activityLogger;
        private readonly IViewModelService<ActivityLog> _activityLogViewModelService;
        private readonly IStringLocalizer<AdminResource> _localizer;

        private readonly string _dataContainer = "activityLog";

        public ActivityLogController(ILoggerService<ActivityLog> activityLogger, IViewModelService<ActivityLog> activityLogViewModelService, IStringLocalizer<AdminResource> localizer)
        {
            _activityLogger = activityLogger;
            _activityLogViewModelService = activityLogViewModelService;
            _localizer = localizer;
        }

        protected IQueryable<ActivityLog> ActivityLogs => _activityLogger.QueryableList;
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            var model = new PageViewModel()
            {
                //Page = PageType.Search,
                PageId = "activityLogSearch",
                Title = _localizer["Activity Logs"].ToString(),
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
                SideBarViewModel = AdminNavPages.ActivityLog
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
                var ams = _activityLogViewModelService.AddCriteria(ActivityLogs, mainSearchFilters);
                var result = await _activityLogViewModelService.CreateViewModelForGrid<ActivityLogSearchResultViewModel>(request, ams, "ActivityDate", "Id");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<ActivityLog>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<ActivityLog>
            {
                Detail = await _activityLogger.GetByIdAsync(id)
            };

            if (viewModel.Detail != null)
            {
                //format form data
                if (!string.IsNullOrEmpty(viewModel.Detail.RequestForm))
                {
                    var requestForm = JsonConvert.DeserializeObject<Dictionary<string, string>>(viewModel.Detail.RequestForm);
                    var formattedData = new List<string>();

                    foreach (var data in requestForm)
                    {
                        formattedData.Add($"{data.Key}: {data.Value}");
                    }

                    viewModel.Detail.RequestForm = string.Join("\r\n", formattedData);
                }

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
                Title = _localizer["Activity Log Detail"].ToString(),
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
                SideBarViewModel = AdminNavPages.ActivityLog
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
            return await GetPicklistData(ActivityLogs, request, property, text, filterType, requiredRelation);
        }
    }
}