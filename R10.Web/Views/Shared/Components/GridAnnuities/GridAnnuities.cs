using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridAnnuities : ViewComponent
    {
        public IViewComponentResult Invoke(AnnuitiesGridOptions model)
        {
            return View(model);
        }
    }

    public class AnnuitiesGridOptions : GridOptions
    {
        public bool CanEditRecord { get; set; }
    }
}
