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
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class CustomFieldsController: BaseController
    {
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CustomFieldsController(IApplicationDbContext repository,
                                      IStringLocalizer<SharedResource> localizer)
        {
            _repository = repository;
            _localizer = localizer;
        }
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Custom Fields"].ToString(),
                PageId = "customFieldsPage",
                MainPartialView = "List",
                //MainViewModel = null,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.CustomFields
            };

            return View("Index", sidebarModel);
        }

        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        [HttpPost]
        public async Task<JsonResult> PageRead([DataSourceRequest] DataSourceRequest request)
        {
            var customFields = _repository.SysCustomFieldSettings.AsNoTracking();

            if (!User.IsSystemEnabled(SystemType.Patent))
                customFields = customFields.Where(f => !((f.TableName ?? "").StartsWith("tblPat")));

            if (!User.IsSystemEnabled(SystemType.Trademark))
                customFields = customFields.Where(f => !((f.TableName ?? "").StartsWith("tblTmk")));

            if (!User.IsSystemEnabled(SystemType.GeneralMatter))
                customFields = customFields.Where(f => !((f.TableName ?? "").StartsWith("tblGM")));

            return Json(await customFields.ToDataSourceResultAsync(request));
        }


        [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        public async Task<IActionResult> Update(
           [Bind(Prefix = "updated")] IList<SysCustomFieldSetting> updated,
           [Bind(Prefix = "new")] IList<SysCustomFieldSetting> added,
           [Bind(Prefix = "deleted")] IList<SysCustomFieldSetting> deleted)
        {
            if (updated.Any() || added.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                if (added.Any())
                {
                    _repository.SysCustomFieldSettings.AddRange(added);
                }

                if (updated.Any())
                {
                    _repository.SysCustomFieldSettings.UpdateRange(updated);
                }
                await _repository.SaveChangesAsync();

                var success = updated.Count() + added.Count() == 1 ?
                _localizer["Custom field has been saved successfully."].ToString() :
                _localizer["Custom fields have been saved successfully"].ToString();
                return Ok(new { success = success });

            }
            return Ok();
        }

        //[Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
        //public async Task<IActionResult> Delete([Bind(Prefix = "deleted")] Core.Entities.Notification deleted)
        //{
        //    if (deleted.MessageId > 0)
        //    {
        //        //requery, if viewed, then timestamp is new
        //        var existing = await _repository.Notifications.FirstOrDefaultAsync(n=> n.MessageId==deleted.MessageId);
        //        _repository.Notifications.Remove(existing);
        //        await _repository.SaveChangesAsync();

        //        if (!existing.Viewed)
        //        {
        //            await _notificationHub.RefreshCount(deleted.UserName);
        //        }

        //        return Ok(new { success = _localizer["Notification has been deleted successfully."].ToString() });
        //    }
        //    return Ok();
        //}


    }
}