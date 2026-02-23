using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class DetailPageButtons : ViewComponent
    {
        public DetailPageButtons()
        {
        }

        public IViewComponentResult Invoke(DetailPagePermission pagePermission, string title)
        {
            ViewBag.Title = title;
            return View(pagePermission);
        }
    }
}
