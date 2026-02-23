using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;

namespace R10.Web.ViewComponents
{
    public class ContextMenu : ViewComponent
    {
        public ContextMenu()
        {
        }

        public IViewComponentResult Invoke(string name, string target, string filter, string onSelect, string onOpen, int width = 100, DetailPagePermission permission = null,
            string showOn = "", ContextMenuOrientation menuOrientationType = ContextMenuOrientation.Horizontal)
        {
            var model = new ContextMenuOptions
            {
                Name = name,
                Target = target,
                Filter = filter,
                MenuOrientationType = menuOrientationType,
                OnSelect = onSelect,
                OnOpen = onOpen,
                ShowOn = showOn,
                Width = width,
                Permission = permission
            };

            return View(model);
        }
    }

    public class ContextMenuOptions
    {
        public string Name { get; set; }
        public string Target { get; set; }
        public string Filter { get; set; }
        public string ShowOn { get; set; }
        public ContextMenuOrientation MenuOrientationType { get; set; }
        public string OnSelect { get; set; }
        public string OnOpen { get; set; }
        public int Width { get; set; }
        public FadeDirection AnimationFadeDirection { get; set; }
        public int AnimationDuration { get; set; }
        public DetailPagePermission Permission { get; set; }

    }
}
