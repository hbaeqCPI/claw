using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Models;
using LawPortal.Web.Security;
using LawPortal.Web.Extensions;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Reflection;
using LawPortal.Core.Entities;
using LawPortal.Web.Models.MenuViewModels;
using LawPortal.Web.Interfaces;
using Newtonsoft.Json;
using LawPortal.Core.Helpers;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using LawPortal.Web.Services;

namespace LawPortal.Web.Controllers
{
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICPiMenuItemManager _menuService;
        private readonly CPiUserManager _userManager;
        private readonly IStringLocalizer<MenuResource> _localizer;
        private readonly IWebHostEnvironment _env;
        private readonly CPiIdentitySettings _cpiSettings;
        private readonly HashSet<string> _validRoutes;

        public HomeController(
            ILogger<HomeController> logger,
            IAuthorizationService authorizationService,
            ICPiMenuItemManager menuService,
            CPiUserManager userManager,
            IStringLocalizer<MenuResource> localizer,
            IWebHostEnvironment env,
            IOptions<CPiIdentitySettings> cpiSettings,
            IActionDescriptorCollectionProvider actionProvider)
        {
            _logger = logger;
            _authorizationService = authorizationService;
            _menuService = menuService;
            _userManager = userManager;
            _localizer = localizer;
            _env = env;
            _cpiSettings = cpiSettings.Value;
            _validRoutes = BuildValidRoutes(actionProvider);
        }

        private static HashSet<string> BuildValidRoutes(IActionDescriptorCollectionProvider actionProvider)
        {
            var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var descriptor in actionProvider.ActionDescriptors.Items)
            {
                if (descriptor is ControllerActionDescriptor cad)
                {
                    cad.RouteValues.TryGetValue("area", out var area);
                    cad.RouteValues.TryGetValue("controller", out var controller);
                    routes.Add($"{area ?? ""}/{controller ?? ""}");
                }
            }
            return routes;
        }

        private bool IsValidMenuItem(CPiMenuItem item)
        {
            if (item.Page == null) return true;
            var area = "";
            if (item.Page.RouteData != null && item.Page.RouteData.TryGetValue("area", out var areaValue))
                area = areaValue?.ToString() ?? "";
            return _validRoutes.Contains($"{area}/{item.Page.Controller}");
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.SSORequired())
                return RedirectToAction("ExternalLogins", "Account");

            if (User.TwoFactorRequired())
                return RedirectToAction("TwoFactorAuthentication", "Account");

            return View();
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

            return RedirectToAction("Index");
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
                        if (!IsValidMenuItem(menuItem)) continue;

                        subMenuItems.Add(new MenuItemViewModel
                        {
                            Title = menuItem.Title,
                            PageId = menuItem.PageId,
                            Page = menuItem.Page,
                            Url = menuItem.Url
                        });
                    }
                    if (subMenuItems.Any())
                    {
                        menuItemViewModel.SubMenuItems = subMenuItems;
                        model.Add(menuItemViewModel);
                    }
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

    }
}
