using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions.ActionResults;
using R10.Web.Extensions;
using Microsoft.Extensions.Localization;
using R10.Web.Services;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Web.Areas.Admin.Helpers;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using R10.Web.Security;
using R10.Web.Api.Models;
using System.Text.Json;
using R10.Core.Identity;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class ScheduledSystemStatusController : BaseController
    {
        private readonly ScheduledTaskService _taskScheduler;
        private readonly IStringLocalizer<TaskSchedulerController> _localizer;
        private const string _requestUri = "api/admin/systemstatus";

        public ScheduledSystemStatusController(ScheduledTaskService taskScheduler, IStringLocalizer<TaskSchedulerController> localizer)
        {
            _taskScheduler = taskScheduler;
            _localizer = localizer;
        }

        private IQueryable<ScheduledTask> Tasks
        {
            get
            {
                var tasks = _taskScheduler.Tasks;

                //hide cpi tasks
                if (!User.IsSuper())
                    tasks = tasks.Where(t => !(t.IsCpiTask ?? false));

                return tasks;
            }
        }

        public async Task<IActionResult> GetTasks([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var tasks = Tasks.Where(t => EF.Functions.Like(t.RequestUri, $"%{_requestUri}%") && t.IsCpiTask == true).ProjectTo<ScheduledSystemStatusListViewModel>();

                if (request.Sorts != null && request.Sorts.Any())
                    tasks = tasks.ApplySorting(request.Sorts);
                else
                    tasks = tasks.OrderBy(p => p.NextRunTime);

                var ids = await tasks.Select(p => p.TaskId).ToArrayAsync();
                var data = await tasks.ApplyPaging(request.Page, request.PageSize).ToListAsync();

                foreach(var task in data)
                {
                    if (string.IsNullOrEmpty(task.RequestContent)) continue;

                    var systemStatusParam = GetSystemStatusParam(task.RequestContent);
                    if (systemStatusParam != null)
                    {
                        task.StatusType = systemStatusParam.SystemStatus?.StatusType.ToString();
                        task.Message = systemStatusParam.SystemStatus?.Notification?.Message;
                        task.ShowNotification = systemStatusParam.SystemStatus?.Notification?.Active;
                        task.UpdateSecurityStamp = systemStatusParam.UpdateSecurityStamp;
                    }
                }

                return Json(new CPiDataSourceResult()
                {
                    Data = data,
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public SystemStatusParam? GetSystemStatusParam(string? requestContent)
        {
            var systemStatusParam = new SystemStatusParam();

            if (!string.IsNullOrEmpty (requestContent))
            {
                try
                {
                    systemStatusParam = JsonSerializer.Deserialize<SystemStatusParam>(requestContent);
                    if (systemStatusParam != null)
                        return systemStatusParam;
                }
                catch { }
            }

            return systemStatusParam;
        }

        public async Task<IActionResult> ViewTask(int id)
        {
            var model = new ScheduledSystemStatusDetailViewModel();

            if (id > 0)
            {
                model = await Tasks.Where(t => t.TaskId == id).ProjectTo<ScheduledSystemStatusDetailViewModel>().FirstOrDefaultAsync();
                if (model == null)
                    return BadRequest(_localizer["Task not found."].Value);
            }
            else
            {
                // required field
                model.RequestUri = _requestUri;
            }

            var systemStatusParam = GetSystemStatusParam(model.RequestContent);
            if (systemStatusParam != null)
            {
                model.StatusType = systemStatusParam.SystemStatus?.StatusType;
                model.Message = systemStatusParam.SystemStatus?.Notification?.Message;
                model.ShowNotification = systemStatusParam.SystemStatus?.Notification?.Active;
                model.UpdateSecurityStamp = systemStatusParam.UpdateSecurityStamp;
            }

            return PartialView("_ScheduledSystemStatus", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTask(ScheduledSystemStatusDetailViewModel updated)
        {
            if (updated == null)
                return BadRequest(_localizer["Task not found."].Value);

            if (string.IsNullOrEmpty(updated.UserName))
                ModelState.AddModelError("updated.UserName", "The UserName field is required");

            if (updated.TaskId == 0 && string.IsNullOrEmpty(updated.Password))
                ModelState.AddModelError("updated.Password", "The Password field is required");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

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

            UpdateEntityStamps(updated, updated.TaskId);

            updated.IsCpiTask = true;
            updated.RequestUri = _requestUri;
            updated.RequestMethod = "POST";
            updated.GrantType = "password";

            var systemStatusParam = new SystemStatusParam()
            {
                SystemStatus = new SystemStatus()
                {
                    StatusType = updated.StatusType ?? SystemStatusType.Active,
                    Notification = new SystemNotification()
                    {
                        Message = updated.Message,
                        Active = updated.ShowNotification ?? false
                    }
                },
                UpdateSecurityStamp = updated.UpdateSecurityStamp ?? false
            };

            updated.RequestContent = JsonSerializer.Serialize(systemStatusParam);

            await _taskScheduler.SaveTask(updated);
            return Json(updated.TaskId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await Tasks.Where(t => t.TaskId == id).FirstOrDefaultAsync();
            if (task == null)
                return BadRequest(_localizer["Task not found."].Value);

            //validate IsCpiTask
            if ((task.IsCpiTask ?? false) && !User.IsSuper())
                return Forbid();

            await _taskScheduler.RemoveTask(task);
            return Ok(_localizer["Record has been deleted successfully."].Value);
        }
    }
}