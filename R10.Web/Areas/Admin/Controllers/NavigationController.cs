using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Models.MenuViewModels;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Web.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using R10.Web.Extensions.ActionResults;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using R10.Core.Helpers;
using R10.Web.Security;
using R10.Web.Areas.Admin.Views;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class NavigationController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ICPiMenuItemManager _menuManager;
        private readonly ICPiMenuPageManager _pageManager;
        private readonly IStringLocalizer<AdminResource> _localizer;

        private List<string> _errors = new List<string>();

        public NavigationController(ICPiMenuItemManager menuManager, ICPiMenuPageManager pageManager, IStringLocalizer<AdminResource> localizer)
        {
            _menuManager = menuManager;
            _pageManager = pageManager;
            _localizer = localizer;
        }
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public async Task<IActionResult> Index(string menuId)
        {
            var mainMenuItems = await _menuManager.GetMenuItemsByParentIdAsync(string.Empty);
            var mainMenu = menuId == null ? mainMenuItems.FirstOrDefault() : mainMenuItems.Find(m => m.Id.ToLower() == menuId.ToLower()) ?? mainMenuItems.FirstOrDefault();

            var model = await GetMenuItems(mainMenu.Id);

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Navigation"].ToString(),
                PageId = "navigationPage",
                MainPartialView = "_Navigation",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Navigation
            };

            ViewData["MenuId"] = mainMenu.Id;
            return View(sidebarModel);
        }

        public async Task<IActionResult> GetMainMenu()
        {
            var mainMenu = await _menuManager.GetMenuItemsByParentIdAsync(string.Empty);
            var topLevelMenus = new List<TopLevelMenuViewModel>();

            foreach (var menu in mainMenu)
            {
                topLevelMenus.Add(new TopLevelMenuViewModel
                {
                    Id = menu.Id,
                    Title = menu.Title
                });
            }
            return Json(topLevelMenus);
        }

        private async Task<List<MenuItemViewModel>> GetMenuItems(string menuId)
        {
            var model = new List<MenuItemViewModel>();
            var subMenus = await _menuManager.GetMenuItemsByParentIdAsync(menuId);

            foreach (var subMenu in subMenus)
            {
                var menuItemViewModel = new MenuItemViewModel
                {
                    Id = subMenu.Id,
                    Title = subMenu.Title,
                    IsEnabled = subMenu.IsEnabled
                };

                var subMenuItems = new List<MenuItemViewModel>();
                var menuItems = await _menuManager.GetMenuItemsByParentIdAsync(subMenu.Id);

                foreach (var menuItem in menuItems)
                {
                    subMenuItems.Add(new MenuItemViewModel
                    {
                        Id = menuItem.Id,
                        Title = menuItem.Title,
                        IsEnabled = menuItem.IsEnabled
                    });
                }
                menuItemViewModel.SubMenuItems = subMenuItems;
                model.Add(menuItemViewModel);
            }
            return model;
        }

        public async Task<IActionResult> GetMenuPages(string areaId)
        {
            List<CPiMenuPage> menuPages = new List<CPiMenuPage>();
            try
            {
                menuPages = await _pageManager.GetAllowedMenuPagesByAreaAsync(areaId);
            }
            catch (Exception e)
            {
                var err = e.Message;
            }
            

            return Json(menuPages);
        }

        public JsonResult GetPolicies(string areaId)
        {
            List<string> authorizationPolicies = new List<string>();

            authorizationPolicies.Add("*");

            if (!string.IsNullOrEmpty(areaId))
            {
                if (areaId.ToLower() == "shared")
                {
                    foreach (var system in User.GetEnabledSystems())
                    {
                        authorizationPolicies.AddRange(GetAuthorizationPolicies(system));
                    }
                }
                else if (areaId.ToLower() != "admin")
                {
                    authorizationPolicies.AddRange(GetAuthorizationPolicies(areaId));
                    authorizationPolicies.AddRange(GetAuthorizationPolicies("Shared"));
                }
            }
            else
            {
                //todo: include all policies if no area in route options??
                authorizationPolicies.AddRange(GetAuthorizationPolicies("Shared"));
            }

            authorizationPolicies.AddRange(GetAuthorizationPolicies("CPi"));

            if (!User.IsSuper())
            {
                authorizationPolicies.Remove(CPiAuthorizationPolicy.CPiAdmin);
            }

            //todo: sort ?
            return Json(authorizationPolicies.Select(s => new SelectListItem { Text = s, Value = s }).OrderBy(o => o.Text));
        }

        public IEnumerable<string> GetAuthorizationPolicies(string systemId)
        {
            string assemblyName = AppDomain.CurrentDomain.GetAssemblies()
                                                .ToList()
                                                .SelectMany(x => x.GetTypes())
                                                .Where(x => x.Name.ToLower() == $"{systemId.ToLower()}authorizationpolicy")
                                                .Select(x => x.AssemblyQualifiedName)
                                                .FirstOrDefault();

            var authorizationPolicies = assemblyName == null ? new List<string>() : Type.GetType(assemblyName)
                                            .GetFields(BindingFlags.Static | BindingFlags.Public)
                                            .Where(f => f.IsLiteral && !f.Name.Contains("RespOffice"))
                                            //.OrderBy(f => f.GetValue(null).ToString())
                                            .Select(f => f.GetValue(null).ToString());

            return authorizationPolicies;
        }

        [HttpPost]
        public Microsoft.AspNetCore.Mvc.PartialViewResult NewPage(string menuId)
        {
            ViewData.Model = new CPiMenuItem()
            {
                ParentId = menuId,
                PageId = 0
            };
            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuItemInfo",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public Microsoft.AspNetCore.Mvc.PartialViewResult NewUrl(string menuId)
        {
            ViewData.Model = new CPiMenuItem()
            {
                ParentId = menuId,
                Url = ""
            };
            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuItemInfo",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public Microsoft.AspNetCore.Mvc.PartialViewResult NewMenu(string menuId)
        {
            ViewData.Model = new CPiMenuItem()
            {
                ParentId = menuId
            };
            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuItemInfo",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.PartialViewResult> MenuItemInfo(string menuId)
        {
            ViewData.Model = await _menuManager.GetMenuItemByIdAsync(menuId);
            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuItemInfo",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.PartialViewResult> NewCard(string menuId)
        {
            var menuItem = await _menuManager.GetMenuItemByIdAsync(menuId);
            ViewData.Model = new MenuItemViewModel()
            {
                Id = menuItem.Id,
                Title = menuItem.Title,
                IsEnabled = menuItem.IsEnabled,
                SubMenuItems = new List<MenuItemViewModel>()
            };

            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuCard",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.PartialViewResult> NewMenuItem(string menuId)
        {
            var menuItem = await _menuManager.GetMenuItemByIdAsync(menuId);
            ViewData.Model = new MenuItemViewModel()
            {
                Id = menuItem.Id,
                Title = menuItem.Title,
                IsEnabled = menuItem.IsEnabled,
                SubMenuItems = new List<MenuItemViewModel>()
            };

            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuItem",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.PartialViewResult> MenuItems(string menuId)
        {
            ViewData.Model =await GetMenuItems(menuId);

            return new Microsoft.AspNetCore.Mvc.PartialViewResult()
            {
                ViewName = "_MenuItems",
                ViewData = ViewData,
                TempData = TempData
            };
        }

        [HttpPost]
        public async Task<JsonResult> SaveMenuItem(CPiMenuItem item)
        {
            CPiMenuItem menuItem;

            var userStamp = User.GetUserName();
            var dateStamp = DateTime.Now;

            if (string.IsNullOrEmpty(item.Id))
            {
                menuItem = new CPiMenuItem()
                {
                    ParentId = item.ParentId,
                    IsEnabled = true,
                    OpenInNewWindow = item.OpenInNewWindow,
                    SortOrder = await _menuManager.GetNextSortOrder(item.ParentId),
                    CreatedBy = userStamp,
                    UpdatedBy = userStamp,
                    DateCreated = dateStamp,
                    LastUpdate = dateStamp
                };
            }
            else
            {
                menuItem = await _menuManager.GetMenuItemByIdAsync(item.Id);
                menuItem.OpenInNewWindow = item.OpenInNewWindow;
                menuItem.UpdatedBy = userStamp;
                menuItem.LastUpdate = dateStamp;
            }

            int pageId = item.PageId ?? 0;
            if (pageId != 0)
            {
                var page = await _pageManager.GetMenuPageByIdAsync(pageId);
                menuItem.Title = page.Name;
                menuItem.Policy = page.Policy;
                menuItem.PageId = page.Id;
            }
            else
            {
                menuItem.Title = item.Title;
                menuItem.Policy = item.Policy;
                menuItem.Url = item.Url;
            }

            try
            {
                menuItem = await _menuManager.SaveMenuItem(menuItem);
                return Json(new { success = true, message = $"{menuItem.Title} successfully saved.", id = menuItem.Id, name = menuItem.Title });
            }
            catch (Exception e)
            {
                _errors.Add(e.Message);
            }
            return Json(new { success = false, message = "Unable to save menu item.", errors = _errors });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteMenuItem(string menuId)
        {
            var menuItem = await _menuManager.GetMenuItemByIdAsync(menuId);
            if (menuItem != null)
            {
                try
                {
                    await _menuManager.RemoveMenuItem(menuItem);

                    return Json(new { success = true, message = $"{menuItem.Title} successfully deleted.", id = menuItem.Id, name = menuItem.Title });
                }
                catch (Exception e)
                {
                    _errors.Add(e.Message);
                }
            }
            else
            {
                _errors.Add("Menu item not found.");
            }

            return Json(new { success = false, message = "Unable to delete menu item.", errors = _errors });
        }

        [HttpPost]
        public async Task<JsonResult> ToggleMenuItem(string menuId, bool isEnabled)
        {
            var menuItem = await _menuManager.GetMenuItemByIdAsync(menuId);
            if (menuItem != null)
            {
                try
                {
                    menuItem.IsEnabled = isEnabled;
                    menuItem.UpdatedBy = User.GetUserName();
                    menuItem.LastUpdate = DateTime.Now;
                    await _menuManager.SaveMenuItem(menuItem);
                    return Json(new { success = true, message = $"{menuItem.Title} has been {(menuItem.IsEnabled ? "enabled" : "disabled")}.", enabled = menuItem.IsEnabled });
                }
                catch (Exception e)
                {
                    _errors.Add(e.Message);
                }
            }
            else
            {
                _errors.Add("Menu item not found.");
            }

            return Json(new { success = false, message = "Unable to update menu item.", errors = _errors });
        }

        [HttpPost]
        public async Task<JsonResult> MoveMenuItem(string menuId, string menuItemid, int newIndex)
        {
            var menuItem = await _menuManager.GetMenuItemByIdAsync(menuItemid);
            if (menuItem != null)
            {
                try
                {
                    menuItem.UpdatedBy = User.GetUserName();
                    menuItem.LastUpdate = DateTime.Now;
                    await _menuManager.MoveMenuItem(menuItem, newIndex, menuId);

                    return Json(new { success = true, message = $"{menuItem.Title} has been updated.", id = menuItem.Id, name = menuItem.Title });
                }
                catch (Exception e)
                {
                    _errors.Add(e.Message);
                }
            }
            else
            {
                _errors.Add("Menu item not found.");
            }

            return Json(new { success = false, message = "Unable to move menu item.", errors = _errors });
        }
    }
}