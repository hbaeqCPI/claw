using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using LawPortal.Core.Entities;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Models.MenuViewModels;

namespace Telerik.Exercise.Shared.Components.Menu
{
    [ViewComponent(Name = "Menu")]
    public class MenuComponent: ViewComponent
    {

        private readonly ICPiMenuItemManager _service;
        private readonly HashSet<string> _validRoutes;

        public MenuComponent(ICPiMenuItemManager service, IActionDescriptorCollectionProvider actionProvider)
        {
            _service = service;
            _validRoutes = BuildValidRoutes(actionProvider);
        }

        /// <summary>
        /// Build a set of "area/controller" keys from all registered controller actions.
        /// Used to filter out menu items that point to deleted controllers.
        /// </summary>
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
            // External links (no Page) are always valid
            if (item.Page == null) return true;

            var area = "";
            if (item.Page.RouteData != null && item.Page.RouteData.TryGetValue("area", out var areaValue))
            {
                area = areaValue?.ToString() ?? "";
            }
            var controller = item.Page.Controller ?? "";
            return _validRoutes.Contains($"{area}/{controller}");
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = new List<MenuItemViewModel>();
            var mainMenu = await _service.GetUserMenuItemsByParentIdAsync(string.Empty);

            foreach (var mainMenuItem in mainMenu)
            {
                var subMenuItems = new List<MenuItemViewModel>();
                var subMenu = await _service.GetUserMenuItemsByParentIdAsync(mainMenuItem.Id);

                if (subMenu.Count() > 0)
                {
                    foreach (var subMenuItem in subMenu)
                    {
                        var menu = await _service.GetUserMenuItemsByParentIdAsync(subMenuItem.Id);
                        if (menu.Count() > 0)
                        {
                            // Filter leaf items to only include those with valid routes
                            var validLeafItems = menu
                                .Where(m => IsValidMenuItem(m))
                                .Select(m => new MenuItemViewModel
                                {
                                    Id = m.Id,
                                    Title = m.Title,
                                    ParentId = m.ParentId,
                                    PageId = m.PageId,
                                    Page = m.Page,
                                    Url = m.Url,
                                    OpenInNewWindow = m.OpenInNewWindow
                                }).ToList();

                            // Only add category if it has valid children
                            if (validLeafItems.Any())
                            {
                                var item = new MenuItemViewModel
                                {
                                    Id = subMenuItem.Id,
                                    Title = subMenuItem.Title,
                                    PageId = subMenuItem.PageId,
                                    Page = subMenuItem.Page,
                                    Url = subMenuItem.Url,
                                    SubMenuItems = validLeafItems
                                };
                                subMenuItems.Add(item);
                            }
                        }
                    }
                }

                // Only add top-level menu if it has valid sub-items
                if (subMenuItems.Any())
                {
                    var menuItem = new MenuItemViewModel
                    {
                        Id = mainMenuItem.Id,
                        Title = mainMenuItem.Title,
                        PageId = mainMenuItem.PageId,
                        Page = mainMenuItem.Page,
                        Url = mainMenuItem.Url,
                        SubMenuItems = subMenuItems
                    };
                    menuItems.Add(menuItem);
                }
            }
            return View("MegaMenu", menuItems);
        }
    }
}
