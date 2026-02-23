using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class GridSchedules : ViewComponent
    {
        public IViewComponentResult Invoke(PageViewModel model)
        {
            return View(model);
        }
    }
}
