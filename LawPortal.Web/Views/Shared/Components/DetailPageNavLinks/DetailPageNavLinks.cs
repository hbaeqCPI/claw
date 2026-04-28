using Microsoft.AspNetCore.Mvc;
using LawPortal.Web.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.ViewComponents
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
