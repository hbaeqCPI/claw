using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Web.Areas.Admin.Helpers;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Admin.Views;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class TaskSchedulerController : BaseController
    {
        private readonly ScheduledTaskService _taskScheduler;
        private readonly IStringLocalizer<TaskSchedulerController> _localizer;

        private string DataContainer => "taskSchedulerDetail";
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public TaskSchedulerController(ScheduledTaskService taskScheduler, IStringLocalizer<TaskSchedulerController> localizer)
        {
            _taskScheduler = taskScheduler;
            _localizer = localizer;
        }

        private IQueryable<ScheduledTask> Tasks {
            get
            {
                var tasks = _taskScheduler.Tasks;

                //hide cpi tasks
                if (!User.IsSuper())
                    tasks = tasks.Where(t => !(t.IsCpiTask ?? false));

                return tasks;
            }
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                PageId = "taskSchedulerSearch",
                Title = _localizer["Task Scheduler"].ToString(),
                CanAddRecord = User.IsSuper()
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.TaskScheduler
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
                var tasks = Tasks.AddCriteria(mainSearchFilters).ProjectTo<ScheduledTaskListViewModel>();

                if (request.Sorts != null && request.Sorts.Any())
                    tasks = tasks.ApplySorting(request.Sorts);
                else
                    tasks = tasks.OrderBy(p => p.Name);

                var ids = await tasks.Select(p => p.TaskId).ToArrayAsync();

                return Json(new CPiDataSourceResult()
                {
                    Data = await tasks.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<ScheduledTaskDetailViewModel>> PrepareEditScreen(int id)
        {
            var detail = await Tasks.Where(m => m.TaskId == id).ProjectTo<ScheduledTaskDetailViewModel>().SingleOrDefaultAsync();
            var viewModel = new DetailPageViewModel<ScheduledTaskDetailViewModel> { Detail = detail };

            if (detail != null)
            {
                var isCPiAdmin = User.IsSuper();

                // CPI tasks can only be edited and deleted from its source
                viewModel.CanAddRecord = isCPiAdmin;
                viewModel.CanDeleteRecord = isCPiAdmin && !(detail.IsCpiTask ?? false);
                viewModel.CanEditRecord = isCPiAdmin && !(detail.IsCpiTask ?? false);
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = DataContainer;

                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });

                viewModel.PageActions = new List<DetailPageAction>() { new DetailPageAction()
                {
                    Label = _localizer["Run"].ToString(),
                    Class = "run-task",
                    IconClass = "fal fa-play",
                    Url = Url.Action("Run"),
                    Data = new Dictionary<string, string>() { 
                        { "id", detail.TaskId.ToString() },
                        { "title", _localizer["Run Task"].Value },
                        { "message", _localizer["Do you want to run this task?"].Value }
                    }
                }};

                detail.BaseUrl = _taskScheduler.BaseUrl;
            }

            return viewModel;
        }

        private DetailPageViewModel<ScheduledTaskDetailViewModel> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<ScheduledTaskDetailViewModel>
            {
                Detail = new ScheduledTaskDetailViewModel() { IsEnabled = true, BaseUrl = _taskScheduler.BaseUrl }
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
                Title = _localizer["Task Detail"].ToString(),
                RecordId = detail.TaskId,
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
                SideBarViewModel = AdminNavPages.TaskScheduler
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
                Title = _localizer["New Task"].ToString(),
                RecordId = detail.TaskId,
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
                SideBarViewModel = AdminNavPages.TaskScheduler
            };

            if (!fromSearch)
            {
                model.Page = PageType.DetailContent;
                return PartialView("_Index", model);
            }

            return PartialView("Index", sidebarModel);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] ScheduledTaskDetailViewModel updated)
        {
            if (ModelState.IsValid)
            {
                if (updated.TaskId > 0)
                {
                    var task = await Tasks.Where(t => t.TaskId == updated.TaskId).FirstOrDefaultAsync();
                    if (task == null)
                        return BadRequest(_localizer["Task not found."].ToString());

                    //validate IsCpiTask
                    if ((task.IsCpiTask ?? false) && !User.IsSuper())
                        return Forbid();

                    //validate password
                    if (string.IsNullOrEmpty(updated.Password))
                    {
                        if (!string.IsNullOrEmpty(task.UserName) && string.Equals(updated.UserName, task.UserName, StringComparison.OrdinalIgnoreCase))
                            //return BadRequest(_localizer["Please re-enter password."].Value);
                            updated.Password = task.Password;
                        else if (!string.IsNullOrEmpty(updated.UserName))
                            return BadRequest(_localizer["Please enter password."].Value);
                    }

                    //ignore non-editable fields
                    updated.Status = task.Status;
                    updated.LastRunTime = task.LastRunTime;
                    updated.LastRunResult = task.LastRunResult;

                    //update status to ready if next run time was updated or if last run failed
                    if (updated.NextRunTime != task.NextRunTime || updated.Status == ScheduledTaskStatus.Failed)
                        updated.Status = ScheduledTaskStatus.Ready;
                }
                else
                {
                    //validate password
                    if (string.IsNullOrEmpty(updated.Password) && !string.IsNullOrEmpty(updated.UserName))
                        return BadRequest(_localizer["Please enter password."].Value);
                }

                if (updated.UseServiceAccount)
                {
                    updated.GrantType = "password";
                    updated.TokenEndpoint = string.Empty;
                    updated.UserName = string.Empty;
                    updated.Password = string.Empty;
                }

                UpdateEntityStamps(updated, updated.TaskId);
                await _taskScheduler.SaveTask(updated);
                return Json(updated.TaskId);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await Tasks.Where(t => t.TaskId == id).FirstOrDefaultAsync();
            if (task == null)
                return BadRequest(_localizer["Task not found."].ToString());

            //validate IsCpiTask
            if ((task.IsCpiTask ?? false) && !User.IsSuper())
                return Forbid();

            await _taskScheduler.RemoveTask(task);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id)
        {
            var task = await Tasks.Where(t => t.TaskId == id).FirstOrDefaultAsync();
            if (task == null)
                return BadRequest(_localizer["Task not found"].Value);

            if ((task.IsCpiTask ?? false) && !User.IsSuper())
                return Forbid();

            var result = await _taskScheduler.RunTask(task);
            if (result.Status == ScheduledTaskStatus.Failed)
                return BadRequest(result.Message);

            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var map = await Tasks.Where(m => m.TaskId == id).FirstOrDefaultAsync();
            if (map == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = map.CreatedBy, dateCreated = map.DateCreated, updatedBy = map.UpdatedBy, lastUpdate = map.LastUpdate, tStamp = map.tStamp });
        }

        public IActionResult VerifyCredentials(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
            {
                return BadRequest($"Please enter password.");
            }

            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(Tasks.ProjectTo<ScheduledTaskListViewModel>(), request, property, text, filterType, requiredRelation);
            return result;
        }
    }
}