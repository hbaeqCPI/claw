using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Extensions;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Reflection;
using R10.Core.Entities;
using R10.Web.Models.MenuViewModels;
using R10.Web.Interfaces;
using Newtonsoft.Json;
using R10.Core.Helpers;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using R10.Web.Services;

namespace R10.Web.Controllers
{
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICPiMenuItemManager _menuService;
        private readonly CPiUserManager _userManager;
        private readonly IStringLocalizer<MenuResource> _localizer;
        private readonly IWebHostEnvironment _env;
        private readonly IBaseService<Help> _helpService;
        private readonly CPiIdentitySettings _cpiSettings;

        public HomeController(
            ILogger<HomeController> logger,
            IAuthorizationService authorizationService,
            ICPiMenuItemManager menuService,
            CPiUserManager userManager,
            IStringLocalizer<MenuResource> localizer,
            IWebHostEnvironment env,
            IBaseService<Help> helpService,
            IOptions<CPiIdentitySettings> cpiSettings)
        {
            _logger = logger;
            _authorizationService = authorizationService;
            _menuService = menuService;
            _userManager = userManager;
            _localizer = localizer;
            _env = env;
            _helpService = helpService;
            _cpiSettings = cpiSettings.Value;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.SSORequired())
                return RedirectToAction("ExternalLogins", "Account");

            if (User.TwoFactorRequired())
                return RedirectToAction("TwoFactorAuthentication", "Account");

            if (!(await _authorizationService.AuthorizeAsync(User, CPiAuthorizationPolicy.DashboardUser)).Succeeded)
                return View();

            try
            {
                var defaultPage = User.GetDefaultPage();
                if (defaultPage != null)
                {
                    if ((defaultPage.SettingPolicy == "*" || (await _authorizationService.AuthorizeAsync(User, defaultPage.SettingPolicy)).Succeeded) &&
                        (defaultPage.PagePolicy == "*" || (await _authorizationService.AuthorizeAsync(User, defaultPage.PagePolicy)).Succeeded))
                    {
                        return RedirectToAction(defaultPage.Action, defaultPage.Controller, defaultPage.Route);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return await RedirectToSystemMenuPage();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessMainMenu)]
        [Route("[action]")]
        public async Task<IActionResult> Patent()
        {
            ViewData["Title"] = _localizer["Patent Management System"].ToString();
            var model = await GetSystemMenuItems(SystemType.Patent);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessMainMenu)]
        [Route("[action]")]
        public async Task<IActionResult> Trademark()
        {
            ViewData["Title"] = _localizer["Trademark Management System"].ToString();
            var model = await GetSystemMenuItems(SystemType.Trademark);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> GeneralMatter()
        {
            ViewData["Title"] = _localizer["General Matters System"].ToString();
            var model = await GetSystemMenuItems(SystemType.GeneralMatter);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = AMSAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> AMS()
        {
            ViewData["Title"] = _localizer["Annuity Management System"].ToString();
            var model = await GetSystemMenuItems(SystemType.AMS);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = DMSAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> DMS()
        {
            ViewData["Title"] = _localizer["Invention Disclosure System"].ToString();
            var model = await GetSystemMenuItems(SystemType.DMS);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = SearchRequestAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> SearchRequest()
        {
            ViewData["Title"] = _localizer["Trademark Search Request"].ToString();
            var model = await GetSystemMenuItems(SystemType.SearchRequest);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = PatentClearanceAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> PatClearance()
        {
            ViewData["Title"] = _localizer["Patent Clearance Search Management System"].ToString();
            var model = await GetSystemMenuItems(SystemType.PatClearance);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = RMSAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> RMS()
        {
            ViewData["Title"] = _localizer["Trademark Renewal Management System"].ToString();
            var model = await GetSystemMenuItems(SystemType.RMS);
            return View("SystemMenu", model);
        }

        [Authorize(Policy = ForeignFilingAuthorizationPolicy.CanAccessSystem)]
        [Route("[action]")]
        public async Task<IActionResult> ForeignFiling()
        {
            ViewData["Title"] = _localizer["Patent Foreign Filing"].ToString();
            var model = await GetSystemMenuItems(SystemType.ForeignFiling);
            return View("SystemMenu", model);
        }

        [Route("[action]")]
        //[Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        public IActionResult About()
        {
            if (_env.IsDevelopment())
                return View();

            return RedirectToAction("Index");
        }

        [Route("[action]")]
        [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
        public IActionResult SysInfo()
        {
            return View("About");
        }

        //[Route("[action]")]
        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        [Route("[action]")]
        public IActionResult Terms()
        {
            return View();
        }

        [Route("[action]")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("[action]")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //to avoid App pool's IdleTimeOut
        public IActionResult Reconnect()
        {
            return Ok();
        }

        private async Task<IActionResult> RedirectToSystemMenuPage()
        {
            var mainMenu = await _menuService.GetUserMenuItemsByParentIdAsync(string.Empty);
            foreach (var menu in mainMenu)
            {
                if (menu.Policy == "*" || (await _authorizationService.AuthorizeAsync(User, menu.Policy)).Succeeded)
                {
                    return RedirectToAction(menu.Id);
                }
            }

            return RedirectToAction("Index", "Dashboard");
        }

        private async Task<List<MenuItemViewModel>> GetSystemMenuItems(string systemType)
        {
            var system = (await _userManager.GetSystems()).Find(s => s.Id.ToLower() == systemType.ToLower());

            if (system == null)
            {
                throw new InvalidOperationException("Unknown system type");
            }

            var model = new List<MenuItemViewModel>();
            var subMenus = await _menuService.GetUserMenuItemsByParentIdAsync(systemType);

            foreach (var subMenu in subMenus)
            {
                var menuItemViewModel = new MenuItemViewModel
                {
                    Title = subMenu.Title
                };

                var subMenuItems = new List<MenuItemViewModel>();
                var menuItems = await _menuService.GetUserMenuItemsByParentIdAsync(subMenu.Id);

                if (menuItems.Count > 0)
                {
                    foreach (var menuItem in menuItems)
                    {
                        subMenuItems.Add(new MenuItemViewModel
                        {
                            Title = menuItem.Title,
                            PageId = menuItem.PageId,
                            Page = menuItem.Page,
                            Url = menuItem.Url
                        });
                    }
                    menuItemViewModel.SubMenuItems = subMenuItems;
                    model.Add(menuItemViewModel);
                }
            }
            return model;
        }

        //temp reports menu placeholder

        //[Route("[action]")]
        //public IActionResult Report(string name)
        //{
        //    if (!string.IsNullOrEmpty(name))
        //        ViewBag.Title = name;

        //    return View();
        //}

        [Authorize]
        public async Task<IActionResult> Help(string? page)
        {
            var clientType = User.GetHelpFolder();
            var path = $"/index.htm";
            var help = await _helpService.QueryableList.Where(h => h.Page == page && ((h.ClientType ?? "") == "" || h.ClientType == clientType)).OrderByDescending(h => h.ClientType).FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(page) && help == null && User.IsSuper())
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                help = new Help()
                {
                    ClientType = clientType,
                    Page = page,
                    Path = "",
                    UpdatedBy = userName,
                    LastUpdate = now,
                    CreatedBy = userName,
                    DateCreated = now,
                };
                await _helpService.Add(help);
            }
            else if (!string.IsNullOrEmpty(help?.Path))
                path = help.Path;

            return Redirect($"{Request.PathBase}/Help/{clientType}{path}");
        }
    }
}
