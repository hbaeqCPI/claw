using Microsoft.AspNetCore.Mvc;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class GridInventors : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }
    }
}
