using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class FilterOperatorList : ViewComponent
    {
        public IViewComponentResult Invoke(string name, Type type = null)
        {
            ViewData["PropertyType"] = type ?? typeof(string);
            return View(model: name);
        }
    }

    public class FilterOperatorListViewModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}
