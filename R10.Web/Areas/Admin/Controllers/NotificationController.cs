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
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models.PageViewModels;
using R10.Core.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    [Area("Admin")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public NotificationController(INotificationService notificationService, IStringLocalizer<SharedResource> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;
        }
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            return List();
        }

        public IActionResult List()
        {
            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Alerts"].ToString(),
                PageId = "notificationListPage",
                MainPartialView = "List",
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Notifications
            };
            return View("Index", sidebarModel);
        }

        public async Task<IActionResult> GetUsers()
        {
            var recipients = await _notificationService.GetRecipients();
            return Json(recipients.OrderBy(r => r.DisplayName).ToList());
        }

        [HttpPost]
        public async Task<JsonResult> PageRead([DataSourceRequest] DataSourceRequest request, bool showViewed)
        {
            var notifications = _notificationService.QueryableList;
            if (!showViewed)
                notifications = notifications.Where(n => !n.Viewed);
            return Json(await notifications.ToDataSourceResultAsync(request));
        }

        public IActionResult GetMessageEntryScreen()
        {
            return PartialView("_MessageEntry");
        }

        [HttpPost]
        public async Task<IActionResult> MessageSave([FromBody] Core.Entities.Notification message)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            base.UpdateEntityStamps(message,message.MessageId);
            await _notificationService.Add(message);

            return Ok();
        }

        public async Task<IActionResult> Update(
           [Bind(Prefix = "updated")]IList<Core.Entities.Notification> updated)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (updated.Any())
            {
                foreach (var message in updated)
                {
                    base.UpdateEntityStamps(message, message.MessageId);
                }
                await _notificationService.Update(updated);
            }
            return Ok();
        }

        public async Task<IActionResult> Delete([Bind(Prefix = "deleted")] Core.Entities.Notification deleted)
        {
            if (deleted.MessageId > 0)
            {
                var notification = await _notificationService.QueryableList.FirstOrDefaultAsync(n => n.MessageId == deleted.MessageId);
                if (notification != null)
                    await _notificationService.Delete(notification);

                return Ok(new { success = _localizer["Alert has been deleted successfully."].ToString() });
            }
            return Ok();
        }       
    }
}