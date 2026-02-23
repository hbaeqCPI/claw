using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class GridScheduleCriteria : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }
    }
}
