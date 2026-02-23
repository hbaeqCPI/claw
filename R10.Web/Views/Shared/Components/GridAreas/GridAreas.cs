using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridAreas : ViewComponent
    {
        public IViewComponentResult Invoke(AreaGridOptions model)
        {
            if (string.IsNullOrEmpty(model.Controller))
                model.Controller = "Country";

            return View(model);
        }
    }

    public class AreaGridOptions : GridOptions
    {
        public string Country { get; set; }
    }
}
