using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.Exceptions;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Models.PageViewModels;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class SettingsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IAuthorizationService _authService;
        private readonly ICPiSystemSettingManager _settingManager;
        private readonly UserManager<CPiUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public SettingsController(
            IAuthorizationService authService,
            ICPiSystemSettingManager settingManager,
            UserManager<CPiUser> userManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _settingManager = settingManager;
            _userManager = userManager;
            _localizer = localizer;
        }
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            return RedirectToAction("Status");
        }

        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        public async Task<IActionResult> Status()
        {
            var model = new SystemSettingsViewModel()
            {
                ActivePage = AdminNavPages.Status,
                ActivePartialViewName = "_Status",
                SystemStatus = await _settingManager.GetSystemSetting<SystemStatus>("")
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Status"].ToString(),
                PageId = "systemSettingsPage",
                MainPartialView = "_Settings",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Status
            };

            return View("Index", sidebarModel);
        }

        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        public async Task<IActionResult> Cookie()
        {
            var model = new SystemSettingsViewModel()
            {
                ActivePage = AdminNavPages.Cookies,
                ActivePartialViewName = "_Cookie",
                CookieConsent = await _settingManager.GetSystemSetting<SystemNotification>("", "CookieConsent")
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Cookie Consent"].ToString(),
                PageId = "systemSettingsPage",
                MainPartialView = "_Settings",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Cookies
            };

            return View("Index", sidebarModel);
        }

        public async Task<IActionResult> ActionIndicator()
        {
            var model = new SystemSettingsViewModel()
            {
                ActivePage = AdminNavPages.ActionIndicator,
                ActivePartialViewName = "_ActionIndicator",
                ActionIndicator = await _settingManager.GetSystemSetting<ActionIndicator>()
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Action Indicator"].ToString(),
                PageId = "systemSettingsPage",
                MainPartialView = "_Settings",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.ActionIndicator
            };

            ViewData["ShowGlobalBadge"] = true;
            return View("Index", sidebarModel);
        }

        public async Task<IActionResult> DeDocketFields()
        {
            if (!(await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.CanAccessDeDocket)).Succeeded)
                return Forbid();

            var model = new SystemSettingsViewModel()
            {
                ActivePage = AdminNavPages.DeDocketFields,
                ActivePartialViewName = "_DeDocketFields",
                DeDocketFields = await _settingManager.GetSystemSetting<DeDocketFields>()
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["DeDocket Fields"].ToString(),
                PageId = "systemSettingsPage",
                MainPartialView = "_Settings",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.DeDocketFields
            };

            ViewData["ShowGlobalBadge"] = true;
            return View("Index", sidebarModel);
        }

        public async Task<IActionResult> InventionDisclosureStatus()
        {
            var model = new SystemSettingsViewModel()
            {
                ActivePage = AdminNavPages.InventionDisclosureStatus,
                ActivePartialViewName = "_InventionDisclosureStatus",
                InventionDisclosureStatus = await _settingManager.GetSystemSetting<InventionDisclosureStatus>()
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Invention Disclosure Status"].ToString(),
                PageId = "systemSettingsPage",
                MainPartialView = "_Settings",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.InventionDisclosureStatus
            };

            ViewData["ShowGlobalBadge"] = false;
            return View("Index", sidebarModel);
        }

        public async Task<IActionResult> Save(string systemId, string settingName, string setting, bool refresh)
        {
            try
            {
                var cpiSetting = await _settingManager.GetCPiSetting(settingName);
                Guard.Against.NoRecordPermission(cpiSetting != null);

                if (!(await _authService.AuthorizeAsync(User, cpiSetting.Policy)).Succeeded)
                    return BadRequest(_localizer["Access denied."].ToString());

                var systemSetting = await _settingManager.QueryableList.Where(s => s.SystemId == (systemId ?? "") && s.SettingId == cpiSetting.Id).FirstOrDefaultAsync();
                JObject settings = JObject.Parse(setting);

                if (systemSetting == null)
                {
                    systemSetting = new CPiSystemSetting()
                    {
                        Id = 0,
                        SystemId = "",
                        SettingId = cpiSetting.Id
                    };
                }
                systemSetting.Settings = JsonConvert.SerializeObject(settings);

                if (systemSetting.Id > 0)
                    await _settingManager.Update(systemSetting);
                else
                    await _settingManager.Add(systemSetting);

                //users will be logged out after ValidationTimeSpan expires
                if (refresh)
                {
                    foreach (var user in await _userManager.Users.ToListAsync())
                    {
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                }

                return Json(new { success = true, message = _localizer["System settings have been saved successfully."].ToString() });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}