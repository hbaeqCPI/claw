using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class DetailPageNavLinks : ViewComponent
    {
        public DetailPageNavLinks()
        {
        }

        public IViewComponentResult Invoke(DetailPagePermission pagePermission, string title, bool addMode)
        {
            ViewBag.Title = title;
            ViewBag.AddMode = addMode;
            return View(pagePermission);
        }
    }
}
