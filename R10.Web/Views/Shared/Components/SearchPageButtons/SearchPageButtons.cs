using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class SearchPageButtons:ViewComponent
    {
        public SearchPageButtons()
        {
        }

        public IViewComponentResult Invoke(bool canAdd, string title)
        {
           ViewBag.CanAddRecord = canAdd;
           ViewBag.Title = title;
           return View();
        }
    }
}
