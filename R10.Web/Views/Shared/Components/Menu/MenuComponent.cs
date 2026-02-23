using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces;
using R10.Web.Interfaces;
using R10.Web.Models.MenuViewModels;

namespace Telerik.Exercise.Shared.Components.Menu
{
    [ViewComponent(Name = "Menu")]
    public class MenuComponent: ViewComponent
    {

        private readonly ICPiMenuItemManager _service;

        public MenuComponent(ICPiMenuItemManager service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = new List<MenuItemViewModel>();
            var mainMenu = await _service.GetUserMenuItemsByParentIdAsync(string.Empty);

            foreach (var mainMenuItem in mainMenu)
            {
                var menuItem = new MenuItemViewModel
                {
                    Id = mainMenuItem.Id,
                    Title = mainMenuItem.Title,
                    PageId = mainMenuItem.PageId,
                    Page = mainMenuItem.Page,
                    Url = mainMenuItem.Url
                };

                var subMenuItems = new List<MenuItemViewModel>();
                var subMenu = await _service.GetUserMenuItemsByParentIdAsync(mainMenuItem.Id);

                if (subMenu.Count() > 0)
                {
                    foreach (var subMenuItem in subMenu)
                    {
                        var menu = await _service.GetUserMenuItemsByParentIdAsync(subMenuItem.Id);
                        if (menu.Count() > 0)
                        {
                            var item = new MenuItemViewModel
                            {
                                Id = subMenuItem.Id,
                                Title = subMenuItem.Title,
                                PageId = subMenuItem.PageId,
                                Page = subMenuItem.Page,
                                Url = subMenuItem.Url,
                                SubMenuItems = menu.Select(m => new MenuItemViewModel
                                {
                                    Id = m.Id,
                                    Title = m.Title,
                                    ParentId = m.ParentId,
                                    PageId = m.PageId,
                                    Page = m.Page,
                                    Url = m.Url,
                                    OpenInNewWindow = m.OpenInNewWindow
                                }).ToList()
                            };
                            subMenuItems.Add(item);
                        }
                    }
                    menuItem.SubMenuItems = subMenuItems;
                }
                menuItems.Add(menuItem);
            }
            return View("MegaMenu", menuItems);
        }


    }
}
